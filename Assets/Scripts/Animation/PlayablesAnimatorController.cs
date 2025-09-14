using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[BurstCompile]
public class PlayablesAnimatorController
{
    private PlayableGraph _playableGraph;
    private readonly AnimationMixerPlayable _mixerPlayable;
    private Playable _currentPlayable;
    private Playable _previousPlayable;
    private float _transitionTime;
    private float _transitionDuration;
   
    private AnimationBlendConfig _currentBlendConfig;
    private AnimationMixerPlayable _blendMixer;
    private readonly List<AnimationClipPlayable> _clipPlayables = new();
    private readonly Dictionary<string, float> _animationParameters = new();

    //cache
    private float _weight;
    private float[] _weights;
    private float _maxWeight;
    private float _totalWeight;
    private float _param;
    private float _distance;
    private Vector2 _paramVector;
    private int _count;
    private AnimationBlendConfig.BlendClip[] _blendClips;
    private AnimationBlendConfig.BlendClip _currentBlendClip;

    // Поля для события по нормализованному времени
    private int _eventClipIndex = -1; // Индекс клипа для события
    private float _eventNormalizedTime = -1f; // Пороговое нормализованное время

    public bool IsActionEnabled { get; private set; } 

    public PlayablesAnimatorController(Animator animator)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is null!");
            return;
        }

        _playableGraph = PlayableGraph.Create("CharacterPlayable");
        _mixerPlayable = AnimationMixerPlayable.Create(_playableGraph, 2);
        var output = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);
        output.SetSourcePlayable(_mixerPlayable);
    }

    public void SetAnimationParameter(string paramName, float value)
    {
        _animationParameters[paramName] = value;
    }

    private float GetAnimationParameter(string paramName)
    {
        return _animationParameters.GetValueOrDefault(paramName);
    }

    // Метод для установки события по нормализованному времени клипа
    private void SetClipTimeEvent(int clipIndex, float normalizedTime)
    {
        if (clipIndex < 0 || clipIndex >= _clipPlayables.Count)
        {
            Debug.LogWarning($"Invalid clip index: {clipIndex}. Must be between 0 and {_clipPlayables.Count - 1}.");
            return;
        }
        if (normalizedTime < 0f || normalizedTime > 1f)
        {
            Debug.LogWarning($"Normalized time must be between 0 and 1, got: {normalizedTime}.");
            return;
        }

        _eventClipIndex = clipIndex;
        _eventNormalizedTime = normalizedTime;
        IsActionEnabled = false;
    }
   
    public void ResetActionFlag()
    {
        IsActionEnabled = false;
    }

    public void OnEnter(AnimationBlendConfig blendConfig, float transitionDuration)
    {
        if (blendConfig == null || blendConfig.Clips == null || blendConfig.Clips.Length == 0)
        {
            return;
        }

        _transitionDuration = transitionDuration;
        _transitionTime = 0;
        _currentBlendConfig = blendConfig;
        
        // Сбрасываем событие при входе в новый бленд
        _eventClipIndex = -1;
        _eventNormalizedTime = -1f;
        IsActionEnabled = false;

        _blendMixer = AnimationMixerPlayable.Create(_playableGraph, blendConfig.Clips.Length);
        _clipPlayables.Clear();

        for (var i = 0; i < blendConfig.Clips.Length; i++)
        {
            var clipData = blendConfig.Clips[i];
            if (clipData.Clip == null)
            {
                Debug.LogWarning($"Clips at index {i} is null!");
                continue;
            }
            var clipPlayable = AnimationClipPlayable.Create(_playableGraph, clipData.Clip);
            _clipPlayables.Add(clipPlayable);
            
            if (!clipData.Clip.isLooping)
            {
                clipPlayable.SetDuration(clipData.Clip.length);
            }
            
            if (clipData.ActionTime > 0 && clipData.ActionTime <= 1)
            {
                SetClipTimeEvent(_clipPlayables.IndexOf(clipPlayable), clipData.ActionTime);
            }
            
            clipPlayable.SetSpeed(clipData.Speed);
            
            _playableGraph.Connect(clipPlayable, 0, _blendMixer, _clipPlayables.Count - 1);
        }

        if (_currentPlayable.IsValid())
        {
            if (_mixerPlayable.GetInput(0).IsValid())
            {
                _playableGraph.Disconnect(_mixerPlayable, 0);
            }
            _previousPlayable = _currentPlayable;
            if (_mixerPlayable.GetInput(1).IsValid())
            {
                _playableGraph.Disconnect(_mixerPlayable, 1);
            }
            _playableGraph.Connect(_previousPlayable, 0, _mixerPlayable, 1);
        }

        _currentPlayable = _blendMixer;
        _playableGraph.Connect(_currentPlayable, 0, _mixerPlayable, 0);

        _mixerPlayable.SetInputWeight(0, 0f); 
        _mixerPlayable.SetInputWeight(1, _previousPlayable.IsValid() ? 1f : 0f); 

        if (!_playableGraph.IsPlaying())
        {
            _playableGraph.Play();
        }

        Update1DWeights();
    }

    public void OnUpdate()
    {
        if (_transitionTime < _transitionDuration)
        {
            _transitionTime += Time.deltaTime;
            _weight = Mathf.Clamp01(_transitionTime / _transitionDuration);
            _mixerPlayable.SetInputWeight(0, _weight); 
            _mixerPlayable.SetInputWeight(1, 1f - _weight); 
        }
        else
        {
            _mixerPlayable.SetInputWeight(0, 1f);
            _mixerPlayable.SetInputWeight(1, 0f);
            Update1DWeights(); 
        }
        UpdateClipTimeEvent();
    }

    private void UpdateClipTimeEvent()
    {
        if (_eventClipIndex < 0 || _eventClipIndex >= _clipPlayables.Count || !_clipPlayables[_eventClipIndex].IsValid())
        {
            return; 
        }

        var clipPlayable = _clipPlayables[_eventClipIndex];
        var clipTime = (float)clipPlayable.GetTime();
        var clipDuration = (float)clipPlayable.GetDuration();

        if (float.IsInfinity(clipDuration) || float.IsNaN(clipDuration) || clipDuration <= 0)
        {
            return;
        }

        var normalizedTime = clipTime / clipDuration;
        if (!IsActionEnabled && normalizedTime >= _eventNormalizedTime)
        {
            IsActionEnabled = true;
        }
    }

    private void Update1DWeights()
    {
        _param = GetAnimationParameter(_currentBlendConfig.ParameterName);
        _blendClips = _currentBlendConfig.Clips.OrderBy(c => c.ParamValue).ToArray();
        _count = _blendClips.Length;

        if (_count == 1)
        {
            _blendMixer.SetInputWeight(0, 1f);
            return;
        }

        if (_param <= _blendClips[0].ParamValue)
        {
            _blendMixer.SetInputWeight(0, 1f);
            for (var i = 1; i < _count; i++) _blendMixer.SetInputWeight(i, 0f);
            return;
        }

        if (_param >= _blendClips[_count - 1].ParamValue)
        {
            _blendMixer.SetInputWeight(_count - 1, 1f);
            for (var i = 0; i < _count - 1; i++) _blendMixer.SetInputWeight(i, 0f);
            return;
        }

        for (var i = 0; i < _count - 1; i++)
        {
            if (_param <= _blendClips[i].ParamValue || _param > _blendClips[i + 1].ParamValue)
            {
                continue;
            }
            var denom = _blendClips[i + 1].ParamValue - _blendClips[i].ParamValue;
            var weightHigh = (_param - _blendClips[i].ParamValue) / (denom == 0 ? 0.0001f : denom);
            var weightLow = 1f - weightHigh;

            _blendMixer.SetInputWeight(i, weightLow);
            _blendMixer.SetInputWeight(i + 1, weightHigh);

            for (var j = 0; j < i; j++) _blendMixer.SetInputWeight(j, 0f);
            for (var j = i + 2; j < _count; j++) _blendMixer.SetInputWeight(j, 0f);
            return;
        }
    }
    
    public void UpdateMoveBlend(float movementX, float movementY)
    {
        _paramVector = new Vector2(movementX, movementY);
        _weights = new float[_currentBlendConfig.Clips.Length];
        
        _totalWeight = 0f;
        _maxWeight = 0f;
        for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            _currentBlendClip = _currentBlendConfig.Clips[i];
            _distance = (_paramVector - _currentBlendClip.ParamPosition).magnitude;
            if (_distance == 0f)
            {
                _weights[i] = float.MaxValue; 
            }
            else
            {
                _weights[i] = 1f / Mathf.Pow(_distance, 4f); 
            }
            _totalWeight += _weights[i];
            if (_weights[i] > _maxWeight) _maxWeight = _weights[i];
        }
        
        var threshold = 0.05f * _maxWeight; 
        _totalWeight = 0f;
        for (var i = 0; i < _weights.Length; i++)
        {
            if (_weights[i] < threshold)
            {
                _weights[i] = 0f;
            }
            _totalWeight += _weights[i];
        }
        
        if (_totalWeight > 0)
        {
            for (var i = 0; i < _weights.Length; i++)
            {
                var normalizedWeight = _weights[i] / _totalWeight;
                _blendMixer.SetInputWeight(i, normalizedWeight);
            }
        }
        else
        {
            for (var i = 0; i < _weights.Length; i++)
            {
                _blendMixer.SetInputWeight(i, 1f / _weights.Length);
            }
        }
    }
    
    private void ClearCurrentBlend()
    {
        if (_blendMixer.IsValid())
        {
            for (var i = 0; i < _clipPlayables.Count; i++)
            {
                if (!_clipPlayables[i].IsValid())
                {
                    continue;
                }
                _playableGraph.Disconnect(_blendMixer, i);
                _clipPlayables[i].Destroy();
            }
            _blendMixer.Destroy();
        }
        _clipPlayables.Clear();
        _eventClipIndex = -1; 
        _eventNormalizedTime = -1f;
        IsActionEnabled = false;
    }

    public void OnDestroy()
    {
        if (!_playableGraph.IsValid())
        {
            return;
        }
        
        if (_mixerPlayable.GetInput(0).IsValid())
        {
            _playableGraph.Disconnect(_mixerPlayable, 0);
        }
        if (_mixerPlayable.GetInput(1).IsValid())
        {
            _playableGraph.Disconnect(_mixerPlayable, 1);
        }

        ClearCurrentBlend();

        if (_currentPlayable.IsValid())
        {
            _currentPlayable.Destroy();
        }
        if (_previousPlayable.IsValid())
        {
            _previousPlayable.Destroy();
        }

        _playableGraph.Destroy();
    }

    public bool IsBlendFinished()
    {
        if (!_currentPlayable.IsValid() || !_blendMixer.IsValid() || _clipPlayables.Count == 0)
        {
            return true;
        }

        for (var i = 0; i < _clipPlayables.Count; i++)
        {
            if (!_clipPlayables[i].IsValid())
            {
                continue;
            }

            var weight = _blendMixer.GetInputWeight(i);
            if (weight <= 0)
            {
                continue;
            }

            var clipPlayable = _clipPlayables[i];
            var clipTime = (float)clipPlayable.GetTime();
            var clipDuration = (float)clipPlayable.GetDuration();

            if (float.IsInfinity(clipDuration) || float.IsNaN(clipDuration) || clipDuration <= 0)
            {
                continue;
            }

            if (clipTime < clipDuration)
            {
                return false;
            }

            if (!clipPlayable.IsDone())
            {
                return false;
            }
        }
        return true;
    }

    public float GetNormalizedBlendTime()
    {
        if (!_currentPlayable.IsValid() || !_blendMixer.IsValid() || _clipPlayables.Count == 0)
        {
            return 0f;
        }

        var totalDuration = 0f;
        var totalTime = 0f;
        var totalWeight = 0f;

        for (var i = 0; i < _clipPlayables.Count; i++)
        {
            if (!_clipPlayables[i].IsValid())
            {
                continue;
            }

            var weight = _blendMixer.GetInputWeight(i);
            if (weight <= 0)
            {
               continue;
            }
            var clipPlayable = _clipPlayables[i];
            var clipDuration = (float)clipPlayable.GetDuration();
            var clipTime = (float)clipPlayable.GetTime();
            totalDuration += clipDuration * weight;
            totalTime += clipTime * weight;
            totalWeight += weight;
        }

        if (totalWeight == 0f || totalDuration == 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(totalTime / totalDuration);
    }
}