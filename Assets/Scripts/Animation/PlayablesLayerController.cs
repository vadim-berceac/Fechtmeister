using System.Linq;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[BurstCompile]
public class PlayablesLayerController
{
    private readonly PlayableGraph _playableGraph;
    private readonly AnimationMixerPlayable _animationMixer;
    private readonly AnimationLayerMixerPlayable _layerMixer;
    private readonly int _layerIndex;

    private AnimationClipPlayable _currentClip;
    private AnimationClipPlayable _previousClip;
    private BlendClip _currentBlendClip;

    private const float FullWeight = 1f;
    private const float ZeroWeight = 0f;

    // Crossfade между клипами
    private float _crossfadeTime;
    private float _crossfadeTimer;
    private bool _isCrossfading;

    // Fade-in всего слоя
    private float _fadeInTime;
    private float _fadeInTimer;
    private float _fadeInStartWeight;
    private bool _isFadingIn;

    // Fade-out всего слоя
    private float _fadeOutTime;
    private float _fadeOutTimer;
    private bool _isFadingOut;

    private bool _actionTimeReached;
    private float _previousNormalizedTime;

    public PlayablesLayerController(PlayableGraph playableGraph,
        AnimationMixerPlayable maskMixer,
        AnimationLayerMixerPlayable layerMixer,
        int layerIndex)
    {
        _playableGraph = playableGraph;
        _animationMixer = maskMixer;
        _layerMixer = layerMixer;
        _layerIndex = layerIndex;
    }

    public void OnUpdate()
    {
        CheckLayerFadeOut();
        CheckLayerFadeIn();
        CheckCrossfade();
        UpdateActionTimeFlag();
    }

    public void PlayAnimationSubState(State state, int configParam, int blendParam)
    {
        var blendClip = GetAnimationClip(state, configParam, blendParam);
        _currentBlendClip = blendClip;

        _isFadingOut = false;
        _fadeInStartWeight = _layerMixer.GetInputWeight(_layerIndex);
        _fadeInTime = state.EnterTransitionDuration > 0f ? state.EnterTransitionDuration : 0.15f;
        _fadeInTimer = 0f;
        _isFadingIn = _fadeInStartWeight < FullWeight;

        if (!_isFadingIn)
            _layerMixer.SetInputWeight(_layerIndex, FullWeight);

        if (_previousClip.IsValid())
        {
            _playableGraph.Disconnect(_animationMixer, 1);
            _previousClip.Destroy();
        }

        if (_currentClip.IsValid())
        {
            _playableGraph.Disconnect(_animationMixer, 0);
            _previousClip = _currentClip;
            _playableGraph.Connect(_previousClip, 0, _animationMixer, 1);
        }
        
        _currentClip = AnimationClipPlayable.Create(_playableGraph, blendClip.Clip);
        _currentClip.SetDuration(blendClip.Clip.length);
        _currentClip.SetSpeed(blendClip.Speed);
        _currentClip.SetTime(0f);
        _currentClip.Play();
        _playableGraph.Connect(_currentClip, 0, _animationMixer, 0);

        _crossfadeTime = state.EnterTransitionDuration;
        _crossfadeTimer = 0f;

        if (_crossfadeTime > 0f && _previousClip.IsValid())
        {
            _isCrossfading = true;
            _animationMixer.SetInputWeight(0, ZeroWeight);
            _animationMixer.SetInputWeight(1, FullWeight);
        }
        else
        {
            _isCrossfading = false;
            _animationMixer.SetInputWeight(0, FullWeight);
            if (_previousClip.IsValid())
                _animationMixer.SetInputWeight(1, ZeroWeight);
        }

        _previousNormalizedTime = 0f;
        _actionTimeReached = false;
    }

    public void StopAnimationSubState(float fadeOutDuration = 0.2f)
    {
        if (_currentClip.IsValid())
            _currentClip.Pause();

        if (_previousClip.IsValid())
            _animationMixer.SetInputWeight(1, ZeroWeight);

        _isCrossfading = false;
        _isFadingIn = false;
        _actionTimeReached = false;
        _previousNormalizedTime = 0f;

        if (fadeOutDuration > 0f)
        {
            _fadeOutTime = fadeOutDuration;
            _fadeOutTimer = 0f;
            _isFadingOut = true;
        }
        else
        {
            _layerMixer.SetInputWeight(_layerIndex, ZeroWeight);
        }
    }

    public bool IsComplete()
    {
        if (!_currentClip.IsValid() || _currentClip.GetAnimationClip() == null)
            return true;

        return _currentClip.GetTime() >= _currentClip.GetDuration() - 0.01f;
    }

    public bool HasReachedActionTime() => _actionTimeReached;

    public void ResetActionTime() => _actionTimeReached = false;

    public void ModifyCurrentWeight(float value)
    {
        var current = _layerMixer.GetInputWeight(_layerIndex);
        _layerMixer.SetInputWeight(_layerIndex, Mathf.Clamp01(current + value));
    }

    public float GetCurrentClipNormalizedTime()
    {
        if (!_currentClip.IsValid() || _currentClip.GetAnimationClip() == null)
            return 0f;

        var duration = (float)_currentClip.GetDuration();
        if (duration <= 0f) return 0f;

        return (float)_currentClip.GetTime() / duration;
    }

    private void CheckCrossfade()
    {
        if (!_isCrossfading) return;

        _crossfadeTimer += Time.deltaTime;
        var t = Mathf.Clamp01(_crossfadeTimer / _crossfadeTime);

        _animationMixer.SetInputWeight(0, t);
        _animationMixer.SetInputWeight(1, 1f - t);

        if (t >= FullWeight)
        {
            _isCrossfading = false;
            _animationMixer.SetInputWeight(0, FullWeight);
            _animationMixer.SetInputWeight(1, ZeroWeight);

            if (_previousClip.IsValid())
            {
                _playableGraph.Disconnect(_animationMixer, 1);
                _previousClip.Destroy();
            }
        }
    }

    private void CheckLayerFadeIn()
    {
        if (!_isFadingIn) return;

        _fadeInTimer += Time.deltaTime;
        var t = Mathf.Clamp01(_fadeInTimer / _fadeInTime);
        _layerMixer.SetInputWeight(_layerIndex, Mathf.Lerp(_fadeInStartWeight, FullWeight, t));

        if (t >= FullWeight)
        {
            _isFadingIn = false;
            _layerMixer.SetInputWeight(_layerIndex, FullWeight);
        }
    }

    private void CheckLayerFadeOut()
    {
        if (!_isFadingOut) return;

        _fadeOutTimer += Time.deltaTime;
        var t = Mathf.Clamp01(_fadeOutTimer / _fadeOutTime);
        _layerMixer.SetInputWeight(_layerIndex, 1f - t);

        if (t >= FullWeight)
        {
            _isFadingOut = false;
            _layerMixer.SetInputWeight(_layerIndex, ZeroWeight);

            if (_currentClip.IsValid())
            {
                _playableGraph.Disconnect(_animationMixer, 0);
                _currentClip.Destroy();
            }

            if (_previousClip.IsValid())
            {
                _playableGraph.Disconnect(_animationMixer, 1);
                _previousClip.Destroy();
            }
        }
    }

    private void UpdateActionTimeFlag()
    {
        if (_currentBlendClip.Clip == null) return;

        var currentNormalized = GetCurrentClipNormalizedTime();
        var actionTime = _currentBlendClip.ActionTime;

        if (IsComplete())
        {
            _actionTimeReached = false;
            _previousNormalizedTime = currentNormalized;
            return;
        }

        if (_currentBlendClip.Clip.isLooping && currentNormalized < _previousNormalizedTime)
            _actionTimeReached = false;

        if (!_actionTimeReached && currentNormalized >= actionTime)
            _actionTimeReached = true;

        _previousNormalizedTime = currentNormalized;
    }

    private static BlendClip GetAnimationClip(State state, int configParam, int blendParam)
    {
        if (state.Clips == null || state.Clips.Length == 0)
            return default;

        var config = state.Clips.FirstOrDefault(con => con.ParamValue == configParam);
        if (config == null)
            return state.Clips[0].Clips[0];

        return config.Clips.FirstOrDefault(con => con.ParamValue == blendParam);
    }
}