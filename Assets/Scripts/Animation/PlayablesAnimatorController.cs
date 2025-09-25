using System;
using System.Linq;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[BurstCompile]
public class PlayablesAnimatorController
{
    private readonly PlayableGraphCore _playableGraphCore;
    public bool IsTransitioning {get; private set;}
    private AnimationMixerPlayable _currentBlendMixer;
    private AnimationMixerPlayable _previousBlendMixer;
    private AnimationBlendConfig _currentBlendConfig;
    private State _currentState;
    private float _transitionTime;
    private float _blendDuration;
    private int _targetClipIndex = -1; 
    private float _clipTransitionTime;
    private const float ClipTransitionDuration = 0.05f; 
    private bool _isClipTransitioning;
    private float[] _targetWeights; 
    private float[] _currentWeights; 
    private float _moveTransitionTime; 
    private bool _isMoveTransitioning;
    private int _currentSlot; 
    private bool _actionTimeReached ; 
    private float _previousNormalizedTime;

    private float _defaultAnimationSpeed;

    public PlayablesAnimatorController(PlayableGraphCore playableGraph)
    {
        _playableGraphCore = playableGraph;
    }

    public void SetAnimationState(State state, int animationBlendParamValue)
    {
        _currentBlendConfig = state.Clips.FirstOrDefault(b => (int)b.ParamValue == animationBlendParamValue);
        if (_currentBlendConfig == null)
        {
            Debug.LogWarning($"Blend config not found for param value: {animationBlendParamValue}");
            return;
        }
       
        if (_currentState == state && _currentBlendMixer.IsValid() && !IsTransitioning)
        {
            var isSameBlend = _currentBlendConfig.Clips
                .Select((clipConfig, i) => new { clipConfig.Clip, Playable = (AnimationClipPlayable)_currentBlendMixer.GetInput(i) })
                .All(x => x.Playable.GetAnimationClip() == x.Clip);

            if (isSameBlend)
            {
                return; 
            }
        }

        _blendDuration = state.EnterTransitionDuration;
        _transitionTime = 0f;
       
        var newBlendMixer = AnimationMixerPlayable.Create(_playableGraphCore.Graph, _currentBlendConfig.Clips.Length);
        
        for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            var clip = _currentBlendConfig.Clips[i];
            var clipPlayable = AnimationClipPlayable.Create(_playableGraphCore.Graph, clip.Clip);
            clipPlayable.SetSpeed(clip.Speed);

            if (!clip.Clip.isLooping)
            {
                clipPlayable.SetDuration(clip.Clip.length);
            }

            _playableGraphCore.Graph.Connect(clipPlayable, 0, newBlendMixer, i);
            newBlendMixer.SetInputWeight(i, 0.0f); 
        }

        var initialClipIndex = _currentBlendConfig.Clips
            .Select((clip, index) => new { clip.ParamValue, index })
            .FirstOrDefault(x => (int)x.ParamValue == animationBlendParamValue)?.index ?? 0;

        newBlendMixer.SetInputWeight(initialClipIndex, 1.0f); 

        _currentWeights = Enumerable.Range(0, _currentBlendConfig.Clips.Length)
            .Select(i => newBlendMixer.GetInputWeight(i))
            .ToArray();

        _targetWeights = Enumerable.Range(0, _currentBlendConfig.Clips.Length)
            .Select(i => newBlendMixer.GetInputWeight(i))
            .ToArray();


        var newSlot = _currentBlendMixer.IsValid() ? 1 - _currentSlot : 0;
        var previousSlot = _currentSlot;

        if (_playableGraphCore.GeneralMixerPlayable.GetInput(newSlot).IsValid())
        {
            _playableGraphCore.Graph.Disconnect(_playableGraphCore.GeneralMixerPlayable, newSlot);
        }

        _playableGraphCore.Graph.Connect(newBlendMixer, 0, _playableGraphCore.GeneralMixerPlayable, newSlot);
        _playableGraphCore.GeneralMixerPlayable.SetInputWeight(previousSlot, _currentBlendMixer.IsValid() ? 1.0f : 0.0f);
        _playableGraphCore.GeneralMixerPlayable.SetInputWeight(newSlot, _currentBlendMixer.IsValid() ? 0.0f : 1.0f);

        if (_currentBlendMixer.IsValid())
        {
            _previousBlendMixer = _currentBlendMixer;
            IsTransitioning = true;
        }

        _currentSlot = newSlot;
        _currentBlendMixer = newBlendMixer;
        _currentState = state;
        _isMoveTransitioning = false;
        _moveTransitionTime = 0f;
        _actionTimeReached = false;
        _previousNormalizedTime = 0f;
        
        _playableGraphCore.Graph.Evaluate();
    }

    public void SetAnimationStateClip(int animationBlendParamValue)
    {
        if (_currentBlendConfig == null || !_currentBlendMixer.IsValid())
            return;

        var targetClipIndex = Array.FindIndex(_currentBlendConfig.Clips,
            clip => (int)clip.ParamValue == animationBlendParamValue);

        if (targetClipIndex == -1 || (_isClipTransitioning && _targetClipIndex == targetClipIndex))
            return;

        _isClipTransitioning = true;
        _clipTransitionTime = 0f;
        _targetClipIndex = targetClipIndex;

        _currentWeights = Enumerable.Range(0, _currentBlendMixer.GetInputCount())
            .Select(i => _currentBlendMixer.GetInputWeight(i))
            .ToArray();

        _targetWeights = Enumerable.Range(0, _currentBlendMixer.GetInputCount())
            .Select(i => i == targetClipIndex ? 1f : 0f)
            .ToArray();

        _actionTimeReached = false;
        _previousNormalizedTime = 0f;

        _playableGraphCore.Graph.Evaluate();
    }


    /// <summary>
    /// Доделать смешивание
    /// </summary>
    public void BlendCurrentAnimationStateClips(float byValue)
    {
        if (_currentBlendConfig == null || !_currentBlendMixer.IsValid())
        {
            Debug.LogWarning("BlendCurrentAnimationStateClips: No valid blend config or mixer available.");
            return;
        }

        _currentWeights = Enumerable.Range(0, _currentBlendMixer.GetInputCount())
            .Select(i => _currentBlendMixer.GetInputWeight(i))
            .ToArray();

        _targetWeights = _currentBlendConfig.Clips
            .Select(clip =>
            {
                var distance = Mathf.Abs(byValue - clip.ParamValue);
                return distance == 0f ? float.MaxValue : 1f / Mathf.Pow(distance, 4f);
            })
            .ToArray();

        var maxWeight = _targetWeights.Max();
        var threshold = 0.05f * maxWeight;

        var filteredWeights = _targetWeights
            .Select(w => w < threshold ? 0f : w)
            .ToArray();

        var totalWeight = filteredWeights.Sum();

        _targetWeights = totalWeight > 0f
            ? filteredWeights.Select(w => w / totalWeight).ToArray()
            : Enumerable.Repeat(1f / _targetWeights.Length, _targetWeights.Length).ToArray();

        _isMoveTransitioning = true;
        _moveTransitionTime = 0f;
        _actionTimeReached = false;
        _previousNormalizedTime = 0f;

        _playableGraphCore.Graph.Evaluate();
    }

    public void Move(float movementX, float movementY)
    {
        if (_currentBlendConfig == null || !_currentBlendMixer.IsValid())
        {
            Debug.LogWarning("Move: No valid blend config or mixer available.");
            return;
        }

        _currentWeights = Enumerable.Range(0, _currentBlendMixer.GetInputCount())
            .Select(i => _currentBlendMixer.GetInputWeight(i))
            .ToArray();

        var paramVector = new Vector2(movementX, movementY);

        _targetWeights = _currentBlendConfig.Clips
            .Select(clip =>
            {
                var distance = (paramVector - clip.ParamPosition).magnitude;
                return distance == 0f ? float.MaxValue : 1f / Mathf.Pow(distance, 4f);
            })
            .ToArray();

        var maxWeight = _targetWeights.Max();
        var threshold = 0.05f * maxWeight;

        var filteredWeights = _targetWeights
            .Select(w => w < threshold ? 0f : w)
            .ToArray();

        var totalWeight = filteredWeights.Sum();

        _targetWeights = totalWeight > 0f
            ? filteredWeights.Select(w => w / totalWeight).ToArray()
            : Enumerable.Repeat(1f / _targetWeights.Length, _targetWeights.Length).ToArray();

        _isMoveTransitioning = true;
        _moveTransitionTime = 0f;
        _actionTimeReached = false;
        _previousNormalizedTime = 0f;

        _playableGraphCore.Graph.Evaluate();
    }

    public void OnUpdate(float deltaTime)
    {
        UpdateActionTimeFlag();

        if (_isClipTransitioning)
        {
            _clipTransitionTime += deltaTime;
            var t = Mathf.Clamp01(_clipTransitionTime / ClipTransitionDuration);
            UpdateMixerWeights(t);

            if (t >= 1f) FinalizeTransition(ref _isClipTransitioning);
            _playableGraphCore.Graph.Evaluate();
        }

        if (_isMoveTransitioning)
        {
            _moveTransitionTime += deltaTime;
            var t = Mathf.Clamp01(_moveTransitionTime / ClipTransitionDuration);
            UpdateMixerWeights(t);

            if (t >= 1f) FinalizeTransition(ref _isMoveTransitioning);
            _playableGraphCore.Graph.Evaluate();
        }

        if (!IsTransitioning) return;

        _transitionTime += deltaTime;
        var stateT = Mathf.Clamp01(_transitionTime / _blendDuration);
        var currentWeight = Mathf.Lerp(0f, 1f, stateT);
        var previousSlot = 1 - _currentSlot;

        _playableGraphCore.GeneralMixerPlayable.SetInputWeight(previousSlot, 1f - currentWeight);
        _playableGraphCore.GeneralMixerPlayable.SetInputWeight(_currentSlot, currentWeight);

        if (stateT < 1f) return;

        IsTransitioning = false;

        if (_previousBlendMixer.IsValid())
        {
            _playableGraphCore.Graph.Disconnect(_playableGraphCore.GeneralMixerPlayable, previousSlot);
            _previousBlendMixer.Destroy();
            _previousBlendMixer = default;
        }

        _playableGraphCore.GeneralMixerPlayable.SetInputWeight(_currentSlot, 1f);
        _playableGraphCore.GeneralMixerPlayable.SetInputWeight(previousSlot, 0f);

        if (!_playableGraphCore.GeneralMixerPlayable.GetInput(_currentSlot).IsValid())
        {
            Debug.LogError($"Current slot {_currentSlot} is invalid after transition!");
        }
        else
        {
            NormalizeMixerWeights();
        }

        _playableGraphCore.Graph.Evaluate();
    }


    private void UpdateMixerWeights(float t)
    {
        var weights = Enumerable.Range(0, _currentBlendMixer.GetInputCount())
            .Select(i => Mathf.Lerp(_currentWeights[i], _targetWeights[i], t))
            .ToArray();

        var sum = weights.Sum();

        weights = Mathf.Abs(sum - 1f) <= 0.001f
            ? weights
            : weights.Select(w => w / sum).ToArray();

        for (var i = 0; i < weights.Length; i++)
            _currentBlendMixer.SetInputWeight(i, weights[i]);
    }

    private void FinalizeTransition(ref bool transitionFlag)
    {
        transitionFlag = false;

        var sum = _targetWeights.Sum();

        var normalized = Mathf.Abs(sum - 1f) <= 0.001f
            ? _targetWeights
            : _targetWeights.Select(w => w / sum).ToArray();

        for (var i = 0; i < normalized.Length; i++)
        {
            _currentBlendMixer.SetInputWeight(i, normalized[i]);
            _currentWeights[i] = normalized[i];
        }
    }

    private void NormalizeMixerWeights()
    {
        var weights = Enumerable.Range(0, _currentBlendMixer.GetInputCount())
            .Select(i => _currentBlendMixer.GetInputWeight(i))
            .ToArray();

        var sum = weights.Sum();

        if (Mathf.Abs(sum - 1f) <= 0.001f) return;

        Debug.LogWarning($"Invalid weights in _currentBlendMixer: sum={sum}. Normalizing...");

        var normalized = weights.Select(w => w / sum).ToArray();

        for (var i = 0; i < normalized.Length; i++)
        {
            _currentBlendMixer.SetInputWeight(i, normalized[i]);
            _currentWeights[i] = normalized[i];
            _targetWeights[i] = normalized[i];
        }
    }
    
    private void UpdateActionTimeFlag()
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
        {
            ResetActionTime();
            return;
        }

        var activeClipIndex = Enumerable.Range(0, _currentBlendMixer.GetInputCount())
            .OrderByDescending(i => _currentBlendMixer.GetInputWeight(i))
            .FirstOrDefault(i => _currentBlendMixer.GetInputWeight(i) > 0f);

        var maxWeight = _currentBlendMixer.GetInputWeight(activeClipIndex);
        if (maxWeight <= 0f)
        {
            ResetActionTime();
            return;
        }

        var clipPlayable = (AnimationClipPlayable)_currentBlendMixer.GetInput(activeClipIndex);
        if (!clipPlayable.IsValid())
        {
            ResetActionTime();
            return;
        }

        var blendClip = _currentBlendConfig.Clips[activeClipIndex];
        var clip = blendClip.Clip;
        var actionTime = blendClip.ActionTime;
        var currentNormalized = GetCurrentClipNormalizedTime();

        if (IsCurrentClipFinished())
        {
            _actionTimeReached = false;
        }
        else
        {
            if (clip.isLooping && currentNormalized < _previousNormalizedTime)
            {
                _actionTimeReached = false;
            }

            if (!_actionTimeReached && currentNormalized >= actionTime)
            {
                _actionTimeReached = true;
            }
        }

        _previousNormalizedTime = currentNormalized;
    }

    private void ResetActionTime()
    {
        _actionTimeReached = false;
        _previousNormalizedTime = 0f;
    }

    public void SetSpeed(float value)
    {
        //получаем в _defaultAnimationSpeed скорость текущего BlendClip
        //выставляем скорость текущего BlendClip равной value
    }

    public void ResetSpeed()
    {
        
    }

    public bool HasReachedActionTime()
    {
        return _actionTimeReached;
    }

    public void ResetActionTimeFlag()
    {
        _actionTimeReached = false;
    }

    public bool IsCurrentClipFinished()
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
            return false;

        var activeClipIndex = Enumerable.Range(0, _currentBlendMixer.GetInputCount())
            .OrderByDescending(i => _currentBlendMixer.GetInputWeight(i))
            .FirstOrDefault(i => _currentBlendMixer.GetInputWeight(i) > 0f);

        var clipPlayable = (AnimationClipPlayable)_currentBlendMixer.GetInput(activeClipIndex);
        if (!clipPlayable.IsValid())
            return false;

        var clip = _currentBlendConfig.Clips[activeClipIndex].Clip;
        if (clip.isLooping)
            return false;

        return clipPlayable.GetTime() >= clipPlayable.GetDuration();
    }


    public float GetCurrentClipNormalizedTime()
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
            return 0f;

        var activeClipIndex = Enumerable.Range(0, _currentBlendMixer.GetInputCount())
            .OrderByDescending(i => _currentBlendMixer.GetInputWeight(i))
            .FirstOrDefault(i => _currentBlendMixer.GetInputWeight(i) > 0f);

        var clipPlayable = (AnimationClipPlayable)_currentBlendMixer.GetInput(activeClipIndex);
        if (!clipPlayable.IsValid())
            return 0f;

        var clip = _currentBlendConfig.Clips[activeClipIndex].Clip;
        var duration = clipPlayable.GetDuration();
        if (duration <= 0.0)
            return 0f;

        var time = clipPlayable.GetTime();
        return clip.isLooping
            ? (float)((time % duration) / duration)
            : Mathf.Clamp01((float)(time / duration));
    }
}