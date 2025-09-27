using System.Collections.Generic;
using UnityEngine.Animations;

public class AnimationState
{
    private readonly string _name;
    private AnimationMixerPlayable _mixerPlayable;
    private readonly HashSet<Dictionary<AnimationClipPlayable, AnimationBlendConfig.BlendClip>> _blendClipGroups;
    
    public AnimationState(State state)
    {
        _name = state.name;
        _blendClipGroups = new HashSet<Dictionary<AnimationClipPlayable, AnimationBlendConfig.BlendClip>>();
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

    public float GetNormalizedDuration()
    {
        return 0f;
    }
}
