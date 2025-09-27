using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimationState
{
    private readonly string _name;
    private readonly HashSet<Dictionary<AnimationClipPlayable, AnimationBlendConfig.BlendClip>> _blendClipGroups;
    
    public AnimationState(PlayableGraph playableGraph, State state)
    {
        _name = state.name;
        _blendClipGroups = new HashSet<Dictionary<AnimationClipPlayable, AnimationBlendConfig.BlendClip>>();

        foreach (var config in state.Clips)
        {
            var dict1 = new Dictionary<AnimationClipPlayable, AnimationBlendConfig.BlendClip>();
            
            foreach (var clip in config.Clips)
            {
                var playableClip = AnimationClipPlayable.Create(playableGraph, clip.Clip);
                playableClip.SetSpeed(clip.Speed);
                
                dict1.Add(playableClip, clip);
            }
            
            _blendClipGroups.Add(dict1);
        }
    }

    public AnimationMixerPlayable GetMixerPlayable(int blendParamValue, int clipParamValue)
    {
        var result = new AnimationMixerPlayable();
        
        return result;
    }

    public string GetName()
    {
        return _name;
    }

    public bool IsComplete()
    {
        return false;
    }

    public float GetNormalizedDuration()
    {
        return 0f;
    }
}
