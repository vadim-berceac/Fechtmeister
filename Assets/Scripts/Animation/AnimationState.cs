using System.Collections.Generic;
using UnityEngine.Animations;

public class AnimationState
{
    private readonly string _name;
    private AnimationMixerPlayable _mixerPlayable;
    private readonly Dictionary<AnimationClipPlayable, AnimationBlendConfig.BlendClip> _blendClips;
    
    public AnimationState (State state)
    {
        _blendClips = new Dictionary<AnimationClipPlayable, AnimationBlendConfig.BlendClip>();
        _name = state.name;
    }

    public AnimationMixerPlayable GetMixerPlayable(int blendParamValue, int clipParamValue)
    {
        return _mixerPlayable;
    }

    public string GetName()
    {
        return _name;
    }

    public bool IsComplete()
    {
        return false;
    }
}
