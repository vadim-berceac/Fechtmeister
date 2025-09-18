using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class CharacterPlayablesAnimatorController
{
    private PlayableGraph _playableGraph;
    private readonly AnimationMixerPlayable _generalMixer;
    private AnimationMixerPlayable _currentBlendMixer;
    private AnimationMixerPlayable _previousBlendMixer;
    private float _transitionTime;
    private float _blendDuration;
    private bool _isTransitioning;
    private State _currentState; // Текущий стейт для предотвращения лишних переходов
    private AnimationBlendConfig _currentBlendConfig;
    private int _targetClipIndex = -1; // Индекс целевого клипа для перехода
    private float _clipTransitionTime = 0f;
    private const float CLIP_TRANSITION_DURATION = 0.05f; // Длительность перехода между клипами
    private bool _isClipTransitioning = false;
    private float[] _targetWeights; // Целевые веса для перехода в Move
    private float[] _currentWeights; // Текущие веса для перехода в Move
    private float _moveTransitionTime = 0f; // Время перехода для Move
    private bool _isMoveTransitioning = false; // Флаг перехода в Move
    private int _currentSlot = 0; // Текущий слот в _generalMixer (0 или 1)

    public CharacterPlayablesAnimatorController(Animator animator)
    {
        _playableGraph = PlayableGraph.Create("CharacterPlayablesAnimatorController");
        var playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Output", animator);
        _generalMixer = AnimationMixerPlayable.Create(_playableGraph, 2);
        playableOutput.SetSourcePlayable(_generalMixer);
        
        _playableGraph.Play();
    }

    public void SetAnimationState(State state, int animationBlendParamValue)
    {
        // Проверяем, существует ли конфигурация для указанного параметра
        _currentBlendConfig = state.Clips.FirstOrDefault(b => (int)b.ParamValue == animationBlendParamValue);
        if (_currentBlendConfig == null)
        {
            Debug.LogWarning($"Blend config not found for param value: {animationBlendParamValue}");
            return;
        }

        // Проверяем, не находимся ли мы уже в этом состоянии с теми же параметрами
        if (_currentState == state && _currentBlendMixer.IsValid() && !_isTransitioning)
        {
            bool isSameBlend = true;
            for (int i = 0; i < _currentBlendConfig.Clips.Length; i++)
            {
                var playable = (AnimationClipPlayable)_currentBlendMixer.GetInput(i);
                if (playable.GetAnimationClip() != _currentBlendConfig.Clips[i].Clip)
                {
                    isSameBlend = false;
                    break;
                }
            }
            if (isSameBlend)
            {
                return; // Уже в нужном состоянии, ничего не делаем
            }
        }

        // Сохраняем длительность перехода
        _blendDuration = state.EnterTransitionDuration;
        _transitionTime = 0f;

        // Создаем новый бленд-миксер
        var newBlendMixer = AnimationMixerPlayable.Create(_playableGraph, _currentBlendConfig.Clips.Length);

        // Настраиваем клипы в новом миксере
        for (int i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            var clip = _currentBlendConfig.Clips[i];
            var clipPlayable = AnimationClipPlayable.Create(_playableGraph, clip.Clip);
            clipPlayable.SetSpeed(clip.Speed);

            if (!clip.Clip.isLooping)
            {
                clipPlayable.SetDuration(clip.Clip.length);
            }

            _playableGraph.Connect(clipPlayable, 0, newBlendMixer, i);
            newBlendMixer.SetInputWeight(i, 0.0f); // Изначально все веса 0
        }

        // Устанавливаем начальный клип для нового миксера
        int initialClipIndex = 0;
        for (int i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            if ((int)_currentBlendConfig.Clips[i].ParamValue == animationBlendParamValue)
            {
                initialClipIndex = i;
                break;
            }
        }
        newBlendMixer.SetInputWeight(initialClipIndex, 1.0f); // Активный клип с весом 1

        // Инициализируем массивы весов для Move
        _targetWeights = new float[_currentBlendConfig.Clips.Length];
        _currentWeights = new float[_currentBlendConfig.Clips.Length];
        for (int i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            _currentWeights[i] = newBlendMixer.GetInputWeight(i);
            _targetWeights[i] = newBlendMixer.GetInputWeight(i); // Синхронизируем целевые веса
        }

        // Подключаем новый миксер
        if (_currentBlendMixer.IsValid())
        {
            // Если есть текущий миксер, начинаем переход
            _previousBlendMixer = _currentBlendMixer;
            int previousSlot = _currentSlot;
            int newSlot = 1 - _currentSlot; // Чередуем слот

            // Отключаем новый слот, если там что-то подключено
            if (_generalMixer.GetInput(newSlot).IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, newSlot);
            }

            // Подключаем новый миксер к новому слоту
            _playableGraph.Connect(newBlendMixer, 0, _generalMixer, newSlot);

            // Устанавливаем начальные веса для перехода
            _generalMixer.SetInputWeight(previousSlot, 1.0f);
            _generalMixer.SetInputWeight(newSlot, 0.0f);

            _isTransitioning = true;
            _currentSlot = newSlot;
        }
        else
        {
            // Первая анимация, подключаем к слоту 0
            if (_generalMixer.GetInput(0).IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, 0);
            }
            _playableGraph.Connect(newBlendMixer, 0, _generalMixer, 0);
            _generalMixer.SetInputWeight(0, 1.0f);
            _generalMixer.SetInputWeight(1, 0.0f);
            _currentSlot = 0;
        }

        // Обновляем текущий миксер и состояние
        _currentBlendMixer = newBlendMixer;
        _currentState = state;

        // Сбрасываем переходы Move, чтобы избежать конфликтов
        _isMoveTransitioning = false;
        _moveTransitionTime = 0f;

        // Немедленно вызываем Evaluate, чтобы избежать разрыва
        _playableGraph.Evaluate();
    }

    public void SetAnimationStateClip(int animationBlendParamValue)
    {
        if (_currentBlendConfig == null || !_currentBlendMixer.IsValid())
        {
            return;
        }

        // Находим индекс целевого клипа
        int targetClipIndex = -1;
        for (int i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            if ((int)_currentBlendConfig.Clips[i].ParamValue == animationBlendParamValue)
            {
                targetClipIndex = i;
                break;
            }
        }

        if (targetClipIndex == -1)
        {
            return; // Клип не найден
        }

        // Если уже переходим к этому клипу, пропускаем
        if (_isClipTransitioning && _targetClipIndex == targetClipIndex)
        {
            return;
        }

        // Начинаем переход к новому клипу
        _isClipTransitioning = true;
        _clipTransitionTime = 0f;
        _targetClipIndex = targetClipIndex;

        // Сохраняем текущие веса
        for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
        {
            _currentWeights[i] = _currentBlendMixer.GetInputWeight(i);
        }

        // Устанавливаем целевые веса: целевой клип — 1, остальные — 0
        for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
        {
            _targetWeights[i] = i == targetClipIndex ? 1.0f : 0.0f;
        }

        _playableGraph.Evaluate();
    }

    public void Move(float movementX, float movementY)
    {
        if (_currentBlendConfig == null || !_currentBlendMixer.IsValid())
        {
            Debug.LogWarning("Move: No valid blend config or mixer available.");
            return;
        }

        // Сохраняем текущие веса перед вычислением новых
        for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
        {
            _currentWeights[i] = _currentBlendMixer.GetInputWeight(i);
        }

        Vector2 paramVector = new Vector2(movementX, movementY);
        float totalWeight = 0f;
        float maxWeight = 0f;

        // Вычисляем целевые веса на основе расстояния до ParamPosition
        for (int i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            var clip = _currentBlendConfig.Clips[i];
            float distance = (paramVector - clip.ParamPosition).magnitude;
            if (distance == 0f)
            {
                _targetWeights[i] = float.MaxValue; // Максимальный вес для точного совпадения
            }
            else
            {
                _targetWeights[i] = 1f / Mathf.Pow(distance, 4f); // Обратная зависимость от расстояния
            }
            totalWeight += _targetWeights[i];
            if (_targetWeights[i] > maxWeight)
            {
                maxWeight = _targetWeights[i];
            }
        }

        // Применяем порог для исключения незначительных весов
        float threshold = 0.05f * maxWeight;
        totalWeight = 0f;
        for (int i = 0; i < _targetWeights.Length; i++)
        {
            if (_targetWeights[i] < threshold)
            {
                _targetWeights[i] = 0f;
            }
            totalWeight += _targetWeights[i];
        }

        // Нормализуем целевые веса или устанавливаем равномерные
        if (totalWeight > 0f)
        {
            for (int i = 0; i < _targetWeights.Length; i++)
            {
                _targetWeights[i] = _targetWeights[i] / totalWeight;
            }
        }
        else
        {
            for (int i = 0; i < _targetWeights.Length; i++)
            {
                _targetWeights[i] = 1f / _targetWeights.Length;
            }
        }

        // Запускаем переход
        _isMoveTransitioning = true;
        _moveTransitionTime = 0f;

        _playableGraph.Evaluate();
    }

    public void OnUpdate(float deltaTime)
    {
        // Обрабатываем переход между клипами в SetAnimationStateClip
        if (_isClipTransitioning)
        {
            _clipTransitionTime += deltaTime;
            float t = Mathf.Clamp01(_clipTransitionTime / CLIP_TRANSITION_DURATION);

            // Интерполируем веса: целевой клип к 1, остальные к 0
            float sumWeights = 0f;
            for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
            {
                float weight = Mathf.Lerp(_currentWeights[i], _targetWeights[i], t);
                _currentBlendMixer.SetInputWeight(i, weight);
                sumWeights += weight;
            }

            // Нормализуем веса, если сумма не равна 1
            if (Mathf.Abs(sumWeights - 1f) > 0.001f)
            {
                for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    float weight = _currentBlendMixer.GetInputWeight(i) / sumWeights;
                    _currentBlendMixer.SetInputWeight(i, weight);
                }
            }

            if (t >= 1.0f)
            {
                _isClipTransitioning = false;
                // Устанавливаем финальные веса
                sumWeights = 0f;
                for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    _currentBlendMixer.SetInputWeight(i, _targetWeights[i]);
                    _currentWeights[i] = _targetWeights[i]; // Обновляем текущие веса
                    sumWeights += _targetWeights[i];
                }

                // Нормализуем финальные веса
                if (Mathf.Abs(sumWeights - 1f) > 0.001f)
                {
                    for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                    {
                        _currentBlendMixer.SetInputWeight(i, _targetWeights[i] / sumWeights);
                        _currentWeights[i] = _targetWeights[i] / sumWeights;
                    }
                }
            }

            _playableGraph.Evaluate();
        }

        // Обрабатываем переход между клипами в Move
        if (_isMoveTransitioning)
        {
            _moveTransitionTime += deltaTime;
            float t = Mathf.Clamp01(_moveTransitionTime / CLIP_TRANSITION_DURATION);

            // Интерполируем веса от текущих к целевым
            float sumWeights = 0f;
            for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
            {
                float weight = Mathf.Lerp(_currentWeights[i], _targetWeights[i], t);
                _currentBlendMixer.SetInputWeight(i, weight);
                sumWeights += weight;
            }

            // Нормализуем веса, если сумма не равна 1
            if (Mathf.Abs(sumWeights - 1f) > 0.001f)
            {
                for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    float weight = _currentBlendMixer.GetInputWeight(i) / sumWeights;
                    _currentBlendMixer.SetInputWeight(i, weight);
                }
            }

            if (t >= 1.0f)
            {
                _isMoveTransitioning = false;
                // Устанавливаем финальные веса
                sumWeights = 0f;
                for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    _currentBlendMixer.SetInputWeight(i, _targetWeights[i]);
                    _currentWeights[i] = _targetWeights[i]; // Обновляем текущие веса
                    sumWeights += _targetWeights[i];
                }

                // Нормализуем финальные веса
                if (Mathf.Abs(sumWeights - 1f) > 0.001f)
                {
                    for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                    {
                        _currentBlendMixer.SetInputWeight(i, _targetWeights[i] / sumWeights);
                        _currentWeights[i] = _targetWeights[i] / sumWeights;
                    }
                }
            }

            _playableGraph.Evaluate();
        }

        // Обрабатываем переход между состояниями
        if (!_isTransitioning)
            return;

        _transitionTime += deltaTime;

        float stateT = Mathf.Clamp01(_transitionTime / _blendDuration);
        float currentWeight = Mathf.Lerp(0f, 1f, stateT);
        
        int previousSlot = 1 - _currentSlot;
        _generalMixer.SetInputWeight(previousSlot, 1f - currentWeight); // Уменьшаем вес предыдущего миксера
        _generalMixer.SetInputWeight(_currentSlot, currentWeight);     // Увеличиваем вес текущего миксера

        if (stateT >= 1f)
        {
            _isTransitioning = false;
            
            // Очищаем предыдущий миксер (отключаем previousSlot)
            if (_previousBlendMixer.IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, previousSlot);
                _previousBlendMixer.Destroy();
                _previousBlendMixer = default; // Сбрасываем ссылку
            }
            _generalMixer.SetInputWeight(_currentSlot, 1.0f);
            _generalMixer.SetInputWeight(1 - _currentSlot, 0.0f);

            // Проверяем, что текущий миксер подключен и имеет валидные веса
            if (!_generalMixer.GetInput(_currentSlot).IsValid())
            {
                Debug.LogError($"Current slot {_currentSlot} is invalid after transition!");
            }
            else
            {
                float sumWeights = 0f;
                for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    sumWeights += _currentBlendMixer.GetInputWeight(i);
                }
                if (Mathf.Abs(sumWeights - 1f) > 0.001f)
                {
                    Debug.LogWarning($"Invalid weights in _currentBlendMixer: sum={sumWeights}. Normalizing...");
                    for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                    {
                        float weight = _currentBlendMixer.GetInputWeight(i) / sumWeights;
                        _currentBlendMixer.SetInputWeight(i, weight);
                        _currentWeights[i] = weight;
                        _targetWeights[i] = weight;
                    }
                }
            }
        }

        _playableGraph.Evaluate();
    }

    public void OnDestroy()
    {
        if (!_playableGraph.IsValid())
        {
            return;
        }
        _playableGraph.Destroy();
    }
}