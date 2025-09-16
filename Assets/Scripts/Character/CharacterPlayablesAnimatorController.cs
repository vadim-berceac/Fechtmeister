using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;

public class CharacterPlayablesAnimatorController
{
    private PlayableGraph _playableGraph;
    private readonly AnimationMixerPlayable _generalMixer;
    private readonly List<AnimationMixerPlayable> _statesMixers;
    private readonly List<AnimationMixerPlayable> _clipsMixers;
    private readonly State[] _states;
    
    public bool IsActionReady { get; private set; }

    public CharacterPlayablesAnimatorController(Animator animator, State[] states)
    {
        _states = states;
        
        _playableGraph = PlayableGraph.Create("CharacterPlayable");
        var animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);
        _generalMixer = AnimationMixerPlayable.Create(_playableGraph, _states.Length);
        animationPlayableOutput.SetSourcePlayable(_generalMixer);
        
        _statesMixers = new List<AnimationMixerPlayable>();
        _clipsMixers = new List<AnimationMixerPlayable>();
        
        for (var i = 0; i < _states.Length; i++)
        {
            var stateMixer = AnimationMixerPlayable.Create(_playableGraph, _states[i].Clips.Length);
            
            for (var j = 0; j < _states[i].Clips.Length; j++)
            {
                var clipCount = _states[i].Clips[j].Clips.Length;
                var clipMixer = AnimationMixerPlayable.Create(_playableGraph, clipCount);
                
                for (var k = 0; k < clipCount; k++)
                {
                    var clipAsset = _states[i].Clips[j].Clips[k].Clip;
                    if (clipAsset == null)
                    {
                        Debug.LogError($"Clip is null in state {_states[i].name}, AnimationBlendConfig {j}, clip index {k}");
                        continue;
                    }

                    var clip = AnimationClipPlayable.Create(_playableGraph, clipAsset);
                    var clipLength = clipAsset.length;
                    if (float.IsNaN(clipLength) || float.IsInfinity(clipLength) || clipLength <= 0)
                    {
                        Debug.LogError($"Invalid length ({clipLength}) for clip {clipAsset.name} in state {_states[i].name}");
                        clipLength = 1f; // Устанавливаем длительность по умолчанию
                    }

                    clip.SetDuration(clipLength);
                    clip.SetSpeed(_states[i].Clips[j].Clips[k].Speed);
                    _playableGraph.Connect(clip, 0, clipMixer, k);
                    clipMixer.SetInputWeight(k, k == 0 ? 1f : 0f);

                    Debug.Log($"Initialized clip {clipAsset.name} in state {_states[i].name}, duration: {clipLength}, looping: {clipAsset.isLooping}");
                }

                _playableGraph.Connect(clipMixer, 0, stateMixer, j);
                _clipsMixers.Add(clipMixer);
                stateMixer.SetInputWeight(j, j == 0 ? 1f : 0f);
            }

            _playableGraph.Connect(stateMixer, 0, _generalMixer, i);
            _statesMixers.Add(stateMixer);
            _generalMixer.SetInputWeight(i, i == 0 ? 1f : 0f);
        }

        _playableGraph.Play();
    }

    public void SelectAnimationState(string stateName)
    {
        if (!_playableGraph.IsValid())
        {
            Debug.LogWarning("PlayableGraph is not valid.");
            return;
        }

        var stateFound = false;
        var clipMixerIndex = 0; 
        for (var i = 0; i < _states.Length; i++)
        {
            if (_states[i].name == stateName)
            {
                _generalMixer.SetInputWeight(i, 1f);
                stateFound = true;
                var stateMixer = _statesMixers[i];
                for (var j = 0; j < stateMixer.GetInputCount(); j++)
                {
                    stateMixer.SetInputWeight(j, j == 0 ? 1f : 0f); 
                    if (clipMixerIndex >= _clipsMixers.Count)
                    {
                        continue;
                    }
                    var clipMixer = _clipsMixers[clipMixerIndex];
                    for (var k = 0; k < clipMixer.GetInputCount(); k++)
                    {
                        clipMixer.SetInputWeight(k, k == 0 ? 1f : 0f); 
                    }
                    clipMixerIndex++;
                }
            }
            else
            {
                _generalMixer.SetInputWeight(i, 0f);
                var stateMixer = _statesMixers[i];
                for (var j = 0; j < stateMixer.GetInputCount(); j++)
                {
                    stateMixer.SetInputWeight(j, 0f);
                    if (clipMixerIndex >= _clipsMixers.Count)
                    {
                       continue;
                    }
                    var clipMixer = _clipsMixers[clipMixerIndex];
                    for (var k = 0; k < clipMixer.GetInputCount(); k++)
                    {
                        clipMixer.SetInputWeight(k, 0f);
                    }
                    clipMixerIndex++;
                }
            }
        }

        if (!stateFound)
        {
            Debug.LogWarning($"State '{stateName}' not found.");
        }
    }

    public void OnUpdate()
    {
        CheckActionConditions();
        SmoothTransitions();
    }

    private void CheckActionConditions()
    {
        if (!_playableGraph.IsValid()) return;

        var clipMixerIndex = 0;
        for (var i = 0; i < _states.Length; i++)
        {
            if (_generalMixer.GetInputWeight(i) > 0f)
            {
                var stateMixer = _statesMixers[i];
                for (var j = 0; j < _states[i].Clips.Length; j++)
                {
                    if (clipMixerIndex >= _clipsMixers.Count)
                    {
                        Debug.LogWarning($"Clip mixer index {clipMixerIndex} exceeds _clipsMixers count {_clipsMixers.Count}");
                        break;
                    }
                    var clipMixer = _clipsMixers[clipMixerIndex];
                    var inputCount = clipMixer.GetInputCount();
                    var clipCount = _states[i].Clips[j].Clips.Length;
                    if (inputCount != clipCount)
                    {
                        Debug.LogWarning($"Mismatch: clipMixer {clipMixerIndex} has {inputCount} inputs, but _states[{i}].Clips[{j}] has {clipCount} clips");
                    }
                    for (var k = 0; k < inputCount && k < clipCount; k++)
                    {
                        if (clipMixer.GetInputWeight(k) > 0f)
                        {
                            var clipPlayable = (AnimationClipPlayable)clipMixer.GetInput(k);
                            var normalizedTime = clipPlayable.GetTime() / clipPlayable.GetDuration();
                            if (normalizedTime >= _states[i].Clips[j].Clips[k].ActionTime)
                            {
                                IsActionReady = true;
                            }
                        }
                    }
                    clipMixerIndex++;
                }
            }
            else
            {
                clipMixerIndex += _states[i].Clips.Length;
            }
        }
    }

    private void SmoothTransitions()
    {
        if (!_playableGraph.IsValid()) return;

        for (var i = 0; i < _states.Length; i++)
        {
            var currentWeight = _generalMixer.GetInputWeight(i);
            var targetWeight = currentWeight > 0.5f ? 1f : 0f;
            var transitionDuration = _states[i].EnterTransitionDuration;
            if (transitionDuration > 0f)
            {
                var smoothedWeight = Mathf.MoveTowards(
                    currentWeight,
                    targetWeight,
                    Time.deltaTime / transitionDuration
                );
                _generalMixer.SetInputWeight(i, smoothedWeight);
            }
            else
            {
                _generalMixer.SetInputWeight(i, targetWeight);
            }
        }
    }

    public void SelectAnimationClip(int paramValue)
    {
        if (!_playableGraph.IsValid()) return;

        for (var i = 0; i < _states.Length; i++)
        {
            if (_generalMixer.GetInputWeight(i) > 0f)
            {
                var stateMixer = _statesMixers[i];
                var clipMixerIndex = 0;
                var selectedConfigIndex = 0;
                var configFound = false;

                // Search for AnimationBlendConfig with matching ParamValue
                for (var j = 0; j < _states[i].Clips.Length; j++)
                {
                    if (Mathf.Approximately(_states[i].Clips[j].ParamValue, paramValue))
                    {
                        selectedConfigIndex = j;
                        configFound = true;
                        break;
                    }
                }

                // Set stateMixer weights and reset clipMixer weights
                for (var j = 0; j < stateMixer.GetInputCount(); j++)
                {
                    if (clipMixerIndex >= _clipsMixers.Count)
                    {
                        Debug.LogWarning($"Clip mixer index {clipMixerIndex} exceeds _clipsMixers count {_clipsMixers.Count}");
                        break;
                    }
                    var clipMixer = _clipsMixers[clipMixerIndex];
                    var inputCount = clipMixer.GetInputCount();
                    var clipCount = _states[i].Clips[j].Clips.Length;
                    if (inputCount != clipCount)
                    {
                        Debug.LogWarning($"Mismatch: clipMixer {clipMixerIndex} has {inputCount} inputs, but _states[{i}].Clips[{j}] has {clipCount} clips");
                    }

                    // Set stateMixer weight
                    stateMixer.SetInputWeight(j, j == selectedConfigIndex && configFound ? 1f : 0f);

                    // Reset clipMixer weights (first clip = 1, others = 0)
                    for (var k = 0; k < inputCount; k++)
                    {
                        clipMixer.SetInputWeight(k, k == 0 ? 1f : 0f);
                    }
                    clipMixerIndex++;
                }

                if (!configFound)
                {
                    Debug.LogWarning($"AnimationBlendConfig with ParamValue {paramValue} not found in state {_states[i].name}");
                }
            }
        }
    }

    public void Move(float movementX, float movementY)
    {
        if (!_playableGraph.IsValid()) return;

        var paramVector = new Vector2(movementX, movementY);
        var clipMixerIndex = 0;
        for (var i = 0; i < _states.Length; i++)
        {
            if (_generalMixer.GetInputWeight(i) > 0f && _states[i].ApplyRootMotion)
            {
                var stateMixer = _statesMixers[i];
                for (var j = 0; j < _states[i].Clips.Length; j++)
                {
                    if (clipMixerIndex >= _clipsMixers.Count)
                    {
                        Debug.LogWarning($"Clip mixer index {clipMixerIndex} exceeds _clipsMixers count {_clipsMixers.Count}");
                        break;
                    }
                    var clipMixer = _clipsMixers[clipMixerIndex];
                    var inputCount = clipMixer.GetInputCount();
                    var clipCount = _states[i].Clips[j].Clips.Length;
                    if (inputCount != clipCount)
                    {
                        Debug.LogWarning($"Mismatch: clipMixer {clipMixerIndex} has {inputCount} inputs, but _states[{i}].Clips[{j}] has {clipCount} clips");
                    }
                    var weights = new float[inputCount];
                    var totalWeight = 0f;
                    var maxWeight = 0f;

                    // Calculate weights based on distance to ParamPosition
                    for (var k = 0; k < inputCount && k < clipCount; k++)
                    {
                        var distance = (paramVector - _states[i].Clips[j].Clips[k].ParamPosition).magnitude;
                        if (distance == 0f)
                        {
                            weights[k] = float.MaxValue;
                        }
                        else
                        {
                            weights[k] = 1f / Mathf.Pow(distance, 4f);
                        }
                        totalWeight += weights[k];
                        if (weights[k] > maxWeight) maxWeight = weights[k];
                    }

                    // Apply threshold to filter out small weights
                    var threshold = 0.05f * maxWeight;
                    totalWeight = 0f;
                    for (var k = 0; k < inputCount && k < clipCount; k++)
                    {
                        if (weights[k] < threshold)
                        {
                            weights[k] = 0f;
                        }
                        totalWeight += weights[k];
                    }

                    // Set normalized weights for the clip mixer
                    for (var k = 0; k < inputCount; k++)
                    {
                        float weight = 0f;
                        if (k < clipCount)
                        {
                            weight = totalWeight > 0f ? weights[k] / totalWeight : 1f / inputCount;
                        }
                        else
                        {
                            Debug.LogWarning($"Index {k} exceeds clip count {clipCount} for clipMixer {clipMixerIndex}");
                        }
                        clipMixer.SetInputWeight(k, weight);
                    }

                    clipMixerIndex++;
                }
            }
            else
            {
                clipMixerIndex += _states[i].Clips.Length;
            }
        }
    }

    public bool IsStateAnimationBlendCompleted()
{
    if (!_playableGraph.IsValid())
    {
        return true; // Если граф недействителен, считаем анимацию завершённой
    }

    bool hasActiveNonLoopingClips = false; // Флаг для отслеживания активных не зацикленных клипов

    for (var i = 0; i < _states.Length; i++)
    {
        if (_generalMixer.GetInputWeight(i) <= 0f)
        {
            continue; // Пропускаем неактивные состояния
        }

        var stateMixer = _statesMixers[i];
        var clipMixerIndex = 0;

        for (var j = 0; j < _states[i].Clips.Length; j++)
        {
            if (clipMixerIndex >= _clipsMixers.Count)
            {
                Debug.LogWarning($"Clip mixer index {clipMixerIndex} exceeds _clipsMixers count {_clipsMixers.Count}");
                break;
            }

            var clipMixer = _clipsMixers[clipMixerIndex];
            var inputCount = clipMixer.GetInputCount();
            var clipCount = _states[i].Clips[j].Clips.Length;

            if (inputCount != clipCount)
            {
                Debug.LogWarning($"Mismatch: clipMixer {clipMixerIndex} has {inputCount} inputs, but _states[{i}].Clips[{j}] has {clipCount} clips");
            }

            for (var k = 0; k < inputCount && k < clipCount; k++)
            {
                if (clipMixer.GetInputWeight(k) <= 0f)
                {
                    continue; // Пропускаем клипы с нулевым весом
                }

                var clipPlayable = (AnimationClipPlayable)clipMixer.GetInput(k);
                var clip = _states[i].Clips[j].Clips[k].Clip;

                if (clip.isLooping)
                {
                    continue; // Пропускаем зацикленные клипы
                }

                hasActiveNonLoopingClips = true; // Нашли активный не зацикленный клип

                var clipTime = (float)clipPlayable.GetTime();
                var clipDuration = (float)clipPlayable.GetDuration();

                if (float.IsInfinity(clipDuration) || float.IsNaN(clipDuration) || clipDuration <= 0)
                {
                    Debug.LogWarning($"Invalid duration for clip {_states[i].Clips[j].Clips[k].Clip.name} in state {_states[i].name}");
                    continue;
                }

                // Если время клипа меньше его длительности, анимация ещё не завершена
                if (clipTime < clipDuration)
                {
                    return false;
                }
            }
            clipMixerIndex++;
        }
    }

    // Возвращаем true, если были найдены активные не зацикленные клипы и все они завершены,
    // или если не было найдено активных не зацикленных клипов
    return hasActiveNonLoopingClips;
}

    public float GetNormalizedStateDuration()
    {
        if (!_playableGraph.IsValid()) return 0f;

        for (var i = 0; i < _states.Length; i++)
        {
            if (_generalMixer.GetInputWeight(i) > 0f)
            {
                var stateMixer = _statesMixers[i];
                var clipMixerIndex = 0;
                for (var j = 0; j < _states[i].Clips.Length; j++)
                {
                    if (clipMixerIndex >= _clipsMixers.Count) break;
                    var clipMixer = _clipsMixers[clipMixerIndex];
                    for (var k = 0; k < clipMixer.GetInputCount(); k++)
                    {
                        if (clipMixer.GetInputWeight(k) > 0f)
                        {
                            var clipPlayable = (AnimationClipPlayable)clipMixer.GetInput(k);
                            return (float)(clipPlayable.GetTime() / clipPlayable.GetDuration());
                        }
                    }
                    clipMixerIndex++;
                }
            }
        }
        return 0f;
    }

    public void ResetAction()
    {
        IsActionReady = false;
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