using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayablesLayerController
{
    private readonly PlayableGraph _playableGraph;
    private readonly AnimationMixerPlayable _animationMixer;
    private AnimationClipPlayable _animationClip;
    public PlayablesLayerController(PlayableGraph playableGraph, AnimationMixerPlayable maskMixer)
    {
        _playableGraph = playableGraph;
        _animationMixer = maskMixer;
    }

    public void OnUpdate()
    {
        
    }

    public void PlayAnimationSubState(State state, int configParam, int blendParam)
    {
        var clip = GetAnimationClip(state, configParam, blendParam);
        if (_animationClip.IsValid())
        {
            _playableGraph.Disconnect(_animationMixer, 0); 
            _animationClip.Destroy(); 
        }
        
        _animationClip = AnimationClipPlayable.Create(_playableGraph, clip);
        _animationClip.SetDuration(clip.length);
        
        _playableGraph.Connect(_animationClip, 0, _animationMixer, 0);
       
        _animationMixer.SetInputWeight(0, 1f);
        _animationClip.SetTime(0f); 
        _animationClip.Play();

        if (!_playableGraph.IsPlaying())
        {
            _playableGraph.Play();
        }
    }

    public void StopAnimationSubState()
    {
        _animationClip.Pause();
        _animationMixer.SetInputWeight(0, 0);
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

    private static AnimationClip GetAnimationClip(State state, int configParam, int blendParam)
    {
        if (state.Clips == null || state.Clips.Length == 0)
        {
            return null;
        }
        var config = state.Clips.FirstOrDefault(con => con.ParamValue == configParam);

        if (config == null)
        {
            return state.Clips[0].Clips[0].Clip;
        }
        
        var blend = config.Clips.FirstOrDefault(con => con.ParamValue == blendParam);

        return  blend.Clip;
    }
}
