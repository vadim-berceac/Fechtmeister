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

        // Подключаем новый миксер
        if (_currentBlendMixer.IsValid())
        {
            // Если есть текущий миксер, начинаем переход
            _previousBlendMixer = _currentBlendMixer;
            _isTransitioning = true;

            // Отключаем существующие соединения, если они есть
            if (_generalMixer.GetInput(0).IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, 0);
            }
            if (_generalMixer.GetInput(1).IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, 1);
            }

            // Подключаем оба миксера к общему миксеру
            _playableGraph.Connect(_previousBlendMixer, 0, _generalMixer, 0);
            _playableGraph.Connect(newBlendMixer, 0, _generalMixer, 1);

            // Устанавливаем начальные веса для перехода
            _generalMixer.SetInputWeight(0, 1.0f); // Предыдущий миксер на полной громкости
            _generalMixer.SetInputWeight(1, 0.0f); // Новый миксер на нулевой громкости
        }
        else
        {
            // Первая анимация, подключаем напрямую
            if (_generalMixer.GetInput(0).IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, 0);
            }
            _playableGraph.Connect(newBlendMixer, 0, _generalMixer, 0);
            _generalMixer.SetInputWeight(0, 1.0f);
        }

        // Обновляем текущий миксер и состояние
        _currentBlendMixer = newBlendMixer;
        _currentState = state;

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

        // Немедленно устанавливаем веса: целевой клип — 1, остальные — 0
        for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
        {
            _currentBlendMixer.SetInputWeight(i, i == targetClipIndex ? 1.0f : 0.0f);
        }

        _playableGraph.Evaluate();
    }

    public void OnUpdate(float deltaTime)
    {
        // Обрабатываем переход между клипами в текущем бленд-миксере
        if (_isClipTransitioning)
        {
            _clipTransitionTime += deltaTime;
            float t = Mathf.Clamp01(_clipTransitionTime / CLIP_TRANSITION_DURATION);

            // Обновляем веса: целевой клип к 1, остальные к 0
            for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
            {
                float weight = (i == _targetClipIndex) ? t : (1.0f - t);
                _currentBlendMixer.SetInputWeight(i, weight);
            }

            if (t >= 1.0f)
            {
                _isClipTransitioning = false;
                // Устанавливаем финальные веса
                for (int i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    _currentBlendMixer.SetInputWeight(i, i == _targetClipIndex ? 1.0f : 0.0f);
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
        
        _generalMixer.SetInputWeight(0, 1f - currentWeight); // Уменьшаем вес предыдущего миксера
        _generalMixer.SetInputWeight(1, currentWeight);     // Увеличиваем вес текущего миксера

        if (stateT >= 1f)
        {
            _isTransitioning = false;
            
            // Очищаем предыдущий миксер (input 0)
            if (_previousBlendMixer.IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, 0);
                _previousBlendMixer.Destroy();
            }

            // Перемещаем текущий миксер с input 1 на input 0
            _playableGraph.Disconnect(_generalMixer, 1);
            _playableGraph.Connect(_currentBlendMixer, 0, _generalMixer, 0);

            // Устанавливаем финальные веса
            _generalMixer.SetInputWeight(0, 1.0f);
            _generalMixer.SetInputWeight(1, 0.0f);
        }

        _playableGraph.Evaluate();
    }

    public void Move(float movementX, float movementY)
    {
        
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