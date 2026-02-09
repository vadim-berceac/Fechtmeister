using System.Linq;
using UnityEngine;

public static class StateExtensions
{
    public static int GetRandomBlendAnimationIndex(this State state, int animationType)
    {
        var animationBlendConfig = state.Clips.FirstOrDefault(a => (int)a.ParamValue == animationType);
    
        if (animationBlendConfig == null || animationBlendConfig.Clips.Length == 0)
            return 0; 
    
        return Random.Range(0, animationBlendConfig.Clips.Length);
    }
}
