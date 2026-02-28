using System;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[BurstCompile]
public class PlayablesAnimatorController
{
    private readonly PlayableGraphCore _playableGraphCore;
    public bool IsTransitioning { get; private set; }
    public int CurrentAnimationBlendParamValue { get; private set; }

    private AnimationMixerPlayable _currentBlendMixer;
    private AnimationMixerPlayable _previousBlendMixer;
    private AnimationBlendConfig _currentBlendConfig;
    private State _currentState;

    private float _transitionTime;
    private float _blendDuration;

    private int _targetClipIndex = -1;
    private float _clipTransitionTime;
    private bool _isClipTransitioning;
    private const float ClipTransitionDuration = 0.05f;

    private float _moveTransitionTime;
    private bool _isMoveTransitioning;

    private float[] _targetWeights;
    private float[] _currentWeights;

    private int _currentSlot;
    private bool _actionTimeReached;
    private float _previousNormalizedTime;

    private float _speedMultiplier = 1f;
    private float _defaultAnimationSpeed = 1f;
    
    private const float DominantWeightThreshold = 0.5f;

    public PlayablesAnimatorController(PlayableGraphCore playableGraph)
    {
        _playableGraphCore = playableGraph;
    }

    public void SetAnimationState(State state, int animationBlendParamValue)
    {
        _currentBlendConfig = System.Linq.Enumerable.FirstOrDefault(state.Clips, b => (int)b.ParamValue == animationBlendParamValue);
        CurrentAnimationBlendParamValue = animationBlendParamValue;

        if (_currentBlendConfig == null)
        {
            return;
        }

        if (_currentState == state && _currentBlendMixer.IsValid() && !IsTransitioning)
        {
            var isSameBlend = true;
            for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
            {
                var playable = (AnimationClipPlayable)_currentBlendMixer.GetInput(i);
                if (playable.GetAnimationClip() != _currentBlendConfig.Clips[i].Clip)
                {
                    isSameBlend = false;
                    break;
                }
            }
            if (isSameBlend) return;
        }

        _blendDuration = state.EnterTransitionDuration;
        _transitionTime = 0f;

        var newBlendMixer = AnimationMixerPlayable.Create(_playableGraphCore.Graph, _currentBlendConfig.Clips.Length);

        for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            var clip = _currentBlendConfig.Clips[i];
            var clipPlayable = AnimationClipPlayable.Create(_playableGraphCore.Graph, clip.Clip);

            clipPlayable.SetSpeed(clip.Speed * _speedMultiplier);

            if (!clip.Clip.isLooping)
                clipPlayable.SetDuration(clip.Clip.length);

            _playableGraphCore.Graph.Connect(clipPlayable, 0, newBlendMixer, i);
            newBlendMixer.SetInputWeight(i, 0f);
        }

        var initialClipIndex = 0;
        for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            if ((int)_currentBlendConfig.Clips[i].ParamValue == animationBlendParamValue)
            {
                initialClipIndex = i;
                break;
            }
        }
        newBlendMixer.SetInputWeight(initialClipIndex, 1f);

        var clipCount = _currentBlendConfig.Clips.Length;
        _currentWeights = new float[clipCount];
        _targetWeights = new float[clipCount];
        for (var i = 0; i < clipCount; i++)
        {
            var w = newBlendMixer.GetInputWeight(i);
            _currentWeights[i] = w;
            _targetWeights[i] = w;
        }

        var newSlot = _currentBlendMixer.IsValid() ? 1 - _currentSlot : 0;
        var previousSlot = _currentSlot;

        if (_playableGraphCore.FullBodyLayerMixer0.GetInput(newSlot).IsValid())
            _playableGraphCore.Graph.Disconnect(_playableGraphCore.FullBodyLayerMixer0, newSlot);

        _playableGraphCore.Graph.Connect(newBlendMixer, 0, _playableGraphCore.FullBodyLayerMixer0, newSlot);

        var hasPrevious = _currentBlendMixer.IsValid();
        _playableGraphCore.FullBodyLayerMixer0.SetInputWeight(previousSlot, hasPrevious ? 1f : 0f);
        _playableGraphCore.FullBodyLayerMixer0.SetInputWeight(newSlot, hasPrevious ? 0f : 1f);

        if (hasPrevious)
        {
            _previousBlendMixer = _currentBlendMixer;
            IsTransitioning = true;
        }

        _currentSlot = newSlot;
        _currentBlendMixer = newBlendMixer;
        _currentState = state;

        _isMoveTransitioning = false;
        _moveTransitionTime = 0f;
        _isClipTransitioning = false;
        _clipTransitionTime = 0f;
        _targetClipIndex = -1;
        _actionTimeReached = false;
        _previousNormalizedTime = 0f;
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

        var count = _currentBlendMixer.GetInputCount();
        EnsureWeightArrays(count);

        for (var i = 0; i < count; i++)
        {
            _currentWeights[i] = _currentBlendMixer.GetInputWeight(i);
            _targetWeights[i] = i == targetClipIndex ? 1f : 0f;
        }

        _actionTimeReached = false;
        _previousNormalizedTime = 0f;
    }

   
    public void BlendCurrentAnimationStateClips(float byValue)
    {
        if (_currentBlendConfig == null || !_currentBlendMixer.IsValid())
        {
            Debug.LogWarning("BlendCurrentAnimationStateClips: No valid blend config or mixer available.");
            return;
        }

        ComputeBlendWeights1D(byValue);
        _isMoveTransitioning = true;
        _moveTransitionTime = 0f;
        _actionTimeReached = false;
        _previousNormalizedTime = 0f;
    }

    public void Move(float movementX, float movementY)
    {
        if (_currentBlendConfig == null || !_currentBlendMixer.IsValid())
        {
            Debug.LogWarning("Move: No valid blend config or mixer available.");
            return;
        }

        ComputeBlendWeights2D(new Vector2(movementX, movementY));
        _isMoveTransitioning = true;
        _moveTransitionTime = 0f;
        _actionTimeReached = false;
        _previousNormalizedTime = 0f;
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
        }

        if (_isMoveTransitioning)
        {
            _moveTransitionTime += deltaTime;
            var t = Mathf.Clamp01(_moveTransitionTime / ClipTransitionDuration);
            UpdateMixerWeights(t);
            if (t >= 1f) FinalizeTransition(ref _isMoveTransitioning);
        }

        if (!IsTransitioning) return;

        _transitionTime += deltaTime;
        var stateT = Mathf.Clamp01(_transitionTime / _blendDuration);
        var currentWeight = Mathf.Lerp(0f, 1f, stateT);
        var previousSlot = 1 - _currentSlot;

        _playableGraphCore.FullBodyLayerMixer0.SetInputWeight(previousSlot, 1f - currentWeight);
        _playableGraphCore.FullBodyLayerMixer0.SetInputWeight(_currentSlot, currentWeight);

        if (stateT < 1f) return;

        IsTransitioning = false;

        if (_previousBlendMixer.IsValid())
        {
            _playableGraphCore.Graph.Disconnect(_playableGraphCore.FullBodyLayerMixer0, previousSlot);
            _previousBlendMixer.Destroy();
            _previousBlendMixer = default;
        }

        _playableGraphCore.FullBodyLayerMixer0.SetInputWeight(_currentSlot, 1f);
        _playableGraphCore.FullBodyLayerMixer0.SetInputWeight(previousSlot, 0f);

        if (!_playableGraphCore.FullBodyLayerMixer0.GetInput(_currentSlot).IsValid())
            Debug.LogError($"Current slot {_currentSlot} is invalid after transition!");
    }

    // --- Speed ---

    private void SetSpeed(float value)
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
            return;

        _defaultAnimationSpeed = _speedMultiplier;
        _speedMultiplier = value;
        ApplySpeedMultiplier(_speedMultiplier);
    }

    private void ResetSpeed()
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
            return;

        _speedMultiplier = _defaultAnimationSpeed;
        ApplySpeedMultiplier(_speedMultiplier);
    }

    private void ApplySpeedMultiplier(float multiplier)
    {
        var count = _currentBlendMixer.GetInputCount();
        for (var i = 0; i < count; i++)
        {
            var clipPlayable = (AnimationClipPlayable)_currentBlendMixer.GetInput(i);
            if (!clipPlayable.IsValid()) continue;

            clipPlayable.SetSpeed(_currentBlendConfig.Clips[i].Speed * multiplier);
        }
    }

    // --- Weights ---

    private void ComputeBlendWeights1D(float byValue)
    {
        var count = _currentBlendMixer.GetInputCount();
        EnsureWeightArrays(count);

        for (var i = 0; i < count; i++)
            _currentWeights[i] = _currentBlendMixer.GetInputWeight(i);

        ComputeInverseDistanceWeights1D(byValue, _targetWeights);
    }

    private void ComputeBlendWeights2D(Vector2 paramVector)
    {
        var count = _currentBlendMixer.GetInputCount();
        EnsureWeightArrays(count);

        for (var i = 0; i < count; i++)
            _currentWeights[i] = _currentBlendMixer.GetInputWeight(i);

        ComputeInverseDistanceWeights2D(paramVector, _targetWeights);
    }

    private void ComputeInverseDistanceWeights1D(float byValue, float[] outWeights)
    {
        var clips = _currentBlendConfig.Clips;
        float maxWeight = 0f;

        for (var i = 0; i < clips.Length; i++)
        {
            var distance = Mathf.Abs(byValue - clips[i].ParamValue);
            outWeights[i] = distance == 0f ? float.MaxValue : 1f / Mathf.Pow(distance, 4f);
            if (outWeights[i] > maxWeight) maxWeight = outWeights[i];
        }

        NormalizeWithThreshold(outWeights, maxWeight);
    }

    private void ComputeInverseDistanceWeights2D(Vector2 paramVector, float[] outWeights)
    {
        var clips = _currentBlendConfig.Clips;
        float maxWeight = 0f;

        for (var i = 0; i < clips.Length; i++)
        {
            var distance = (paramVector - clips[i].ParamPosition).magnitude;
            outWeights[i] = distance == 0f ? float.MaxValue : 1f / Mathf.Pow(distance, 4f);
            if (outWeights[i] > maxWeight) maxWeight = outWeights[i];
        }

        NormalizeWithThreshold(outWeights, maxWeight);
    }

    private static void NormalizeWithThreshold(float[] weights, float maxWeight)
    {
        var threshold = 0.05f * maxWeight;
        float total = 0f;

        for (var i = 0; i < weights.Length; i++)
        {
            if (weights[i] < threshold) weights[i] = 0f;
            total += weights[i];
        }

        if (total > 0f)
        {
            for (var i = 0; i < weights.Length; i++)
                weights[i] /= total;
        }
        else
        {
            var uniform = 1f / weights.Length;
            for (var i = 0; i < weights.Length; i++)
                weights[i] = uniform;
        }
    }

    private void EnsureWeightArrays(int count)
    {
        if (_currentWeights == null || _currentWeights.Length != count)
            _currentWeights = new float[count];
        if (_targetWeights == null || _targetWeights.Length != count)
            _targetWeights = new float[count];
    }

    private void UpdateMixerWeights(float t)
    {
        var count = _currentBlendMixer.GetInputCount();
        float sum = 0f;

        for (var i = 0; i < count; i++)
        {
            var w = Mathf.Lerp(_currentWeights[i], _targetWeights[i], t);
            _currentWeights[i] = w;
            sum += w;
        }

        var invSum = Mathf.Abs(sum - 1f) <= 0.001f ? 1f : (sum > 0f ? 1f / sum : 0f);
        for (var i = 0; i < count; i++)
            _currentBlendMixer.SetInputWeight(i, _currentWeights[i] * invSum);
    }

    private void FinalizeTransition(ref bool transitionFlag)
    {
        transitionFlag = false;

        float sum = 0f;
        for (var i = 0; i < _targetWeights.Length; i++)
            sum += _targetWeights[i];

        var invSum = Mathf.Abs(sum - 1f) <= 0.001f ? 1f : (sum > 0f ? 1f / sum : 0f);

        for (var i = 0; i < _targetWeights.Length; i++)
        {
            var w = _targetWeights[i] * invSum;
            _currentBlendMixer.SetInputWeight(i, w);
            _currentWeights[i] = w;
            _targetWeights[i] = w;
        }
    }

    // --- Clip helpers ---

    private int GetActiveClipIndex()
    {
        var count = _currentBlendMixer.GetInputCount();
        var bestIndex = 0;
        var bestWeight = -1f;

        for (var i = 0; i < count; i++)
        {
            var w = _currentBlendMixer.GetInputWeight(i);
            if (w > bestWeight)
            {
                bestWeight = w;
                bestIndex = i;
            }
        }

        return bestWeight > 0f ? bestIndex : -1;
    }

    private int GetDominantClipIndex()
    {
        var count = _currentBlendMixer.GetInputCount();
        var bestIndex = -1;
        var bestWeight = -1f;

        for (var i = 0; i < count; i++)
        {
            var w = _currentBlendMixer.GetInputWeight(i);
            if (w > bestWeight)
            {
                bestWeight = w;
                bestIndex = i;
            }
        }

        return bestWeight >= DominantWeightThreshold ? bestIndex : -1;
    }

    private static bool IsClipFinished(AnimationClipPlayable playable, AnimationClip clip)
    {
        if (clip.isLooping) return false;
        return playable.GetTime() >= playable.GetDuration();
    }

    private static float ComputeNormalizedTime(AnimationClipPlayable playable, AnimationClip clip)
    {
        var duration = playable.GetDuration();
        if (duration <= 0.0) return 0f;

        var time = playable.GetTime();
        return clip.isLooping
            ? (float)((time % duration) / duration)
            : Mathf.Clamp01((float)(time / duration));
    }

    // --- Action time ---

    private void UpdateActionTimeFlag()
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
        {
            ResetActionTime();
            return;
        }

        var dominantClipIndex = GetDominantClipIndex();
        if (dominantClipIndex == -1)
        {
            ResetActionTime();
            return;
        }

        var clipPlayable = (AnimationClipPlayable)_currentBlendMixer.GetInput(dominantClipIndex);
        if (!clipPlayable.IsValid())
        {
            ResetActionTime();
            return;
        }

        var blendClip = _currentBlendConfig.Clips[dominantClipIndex];
        var currentNormalized = ComputeNormalizedTime(clipPlayable, blendClip.Clip);

        if (IsClipFinished(clipPlayable, blendClip.Clip))
        {
            _actionTimeReached = false;
        }
        else
        {
            if (blendClip.Clip.isLooping && currentNormalized < _previousNormalizedTime)
                _actionTimeReached = false;

            if (!_actionTimeReached && currentNormalized >= blendClip.ActionTime)
                _actionTimeReached = true;
        }

        _previousNormalizedTime = currentNormalized;
    }

    private void ResetActionTime()
    {
        _actionTimeReached = false;
        _previousNormalizedTime = 0f;
    }

    // --- Public API ---

    public bool IsCurrentClipFinished()
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
            return false;

        var dominantClipIndex = GetDominantClipIndex();
        if (dominantClipIndex == -1) return false;

        var clipPlayable = (AnimationClipPlayable)_currentBlendMixer.GetInput(dominantClipIndex);
        if (!clipPlayable.IsValid()) return false;

        return IsClipFinished(clipPlayable, _currentBlendConfig.Clips[dominantClipIndex].Clip);
    }

    public float GetCurrentClipNormalizedTime()
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
            return 0f;

        var dominantClipIndex = GetDominantClipIndex();
        if (dominantClipIndex == -1) return 0f;

        var clipPlayable = (AnimationClipPlayable)_currentBlendMixer.GetInput(dominantClipIndex);
        if (!clipPlayable.IsValid()) return 0f;

        return ComputeNormalizedTime(clipPlayable, _currentBlendConfig.Clips[dominantClipIndex].Clip);
    }

    public bool HasReachedActionTime() => _actionTimeReached;
    public void ResetActionTimeFlag() => _actionTimeReached = false;
}