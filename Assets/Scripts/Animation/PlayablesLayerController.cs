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
    private AnimationClipPlayable _animationClip;
    private BlendClip _currentBlendClip; 
    private const float FullWeight = 1f;
    private const float ZeroWeight = 0f;
    private float _currentWeight;
    private float _blendTime;
    private float _blendTimer;
    private bool _isBlending;
    private bool _actionTimeReached;
    private float _previousNormalizedTime; 

    public PlayablesLayerController(PlayableGraph playableGraph, AnimationMixerPlayable maskMixer)
    {
        _playableGraph = playableGraph;
        _animationMixer = maskMixer;
        _currentWeight = 0f;
        _blendTime = 0f;
        _blendTimer = 0f;
        _isBlending = false;
        _previousNormalizedTime = 0f;
    }

    public void OnUpdate()
    {
        CheckBlending();
        UpdateActionTimeFlag();
    }

    public void PlayAnimationSubState(State state, int configParam, int blendParam)
    {
        var blendClip = GetAnimationClip(state, configParam, blendParam);
        _currentBlendClip = blendClip; 

        if (_animationClip.IsValid())
        {
            _playableGraph.Disconnect(_animationMixer, 0);
            _animationClip.Destroy();
        }

        _animationClip = AnimationClipPlayable.Create(_playableGraph, blendClip.Clip);
        _animationClip.SetDuration(blendClip.Clip.length);
        _animationClip.SetSpeed(blendClip.Speed);

        _playableGraph.Connect(_animationClip, 0, _animationMixer, 0);

        _blendTime = state.EnterTransitionDuration;
        _blendTimer = 0f;
        _currentWeight = _animationMixer.GetInputWeight(0);
        _isBlending =  state.EnterTransitionDuration > 0f;

        if (!_isBlending)
        {
            _animationMixer.SetInputWeight(0, FullWeight);
        }

        _animationClip.SetTime(0f);
        _animationClip.Play();
        _previousNormalizedTime = 0f; 
        _actionTimeReached = false; 
    }

    public void StopAnimationSubState()
    {
        _animationClip.Pause();
        _animationMixer.SetInputWeight(0, ZeroWeight);
        _actionTimeReached = false; 
        _previousNormalizedTime = 0f;
    }

    public bool IsComplete()
    {
        if (!_animationClip.IsValid())
        {
            return true;
        }

        var clip = _animationClip.GetAnimationClip();
        if (clip == null)
        {
            return true;
        }
        return _animationClip.GetTime() >= _animationClip.GetDuration() - 0.01f;
    }

    public bool HasReachedActionTime()
    {
        return _actionTimeReached;
    }

    public void ResetActionTime()
    {
        _actionTimeReached = false;
    }

    public void ModifyCurrentWeight(float value)
    {
        _currentWeight += value;
    }

    public float GetCurrentClipNormalizedTime()
    {
        if (!_animationClip.IsValid() || _animationClip.GetAnimationClip() == null)
        {
            return 0f;
        }

        var duration = (float)_animationClip.GetDuration();
        if (duration <= 0f)
        {
            return 0f;
        }

        var currentTime = (float)_animationClip.GetTime();
        return currentTime / duration;
    }

    private void UpdateActionTimeFlag()
    {
        var currentNormalized = GetCurrentClipNormalizedTime();
        var clip = _currentBlendClip.Clip;
        var actionTime = _currentBlendClip.ActionTime;
       
        if (IsComplete())
        {
            _actionTimeReached = false;
            _previousNormalizedTime = currentNormalized;
            return;
        }
       
        if (clip.isLooping && currentNormalized < _previousNormalizedTime)
        {
            _actionTimeReached = false; 
        }
        
        if (!_actionTimeReached && currentNormalized >= actionTime)
        {
            //Debug.Log(_actionTimeReached + " - " + currentNormalized + " - " + actionTime);
            _actionTimeReached = true;
        }

        _previousNormalizedTime = currentNormalized;
    }

    private void CheckBlending()
    {
        if (!_isBlending)
        {
            return;
        }
        _blendTimer += Time.deltaTime;
        var t = Mathf.Clamp01(_blendTimer / _blendTime);
        _currentWeight = Mathf.Lerp(_currentWeight, FullWeight, t);
        if (_animationClip.IsValid())
        {
            _animationMixer.SetInputWeight(0, _currentWeight);
        }

        if (t >= FullWeight)
        {
            _isBlending = false;
        }
    }

    private static BlendClip GetAnimationClip(State state, int configParam, int blendParam)
    {
        if (state.Clips == null || state.Clips.Length == 0)
        {
            return default;
        }
        var config = state.Clips.FirstOrDefault(con => con.ParamValue == configParam);

        if (config == null)
        {
            return state.Clips[0].Clips[0];
        }

        return config.Clips.FirstOrDefault(con => con.ParamValue == blendParam);
    }
}