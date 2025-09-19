using System.Linq;

public static class StateExtensions
{
    public static int GetBlendAnimationsCount(this State state, int animationType)
    {
        var animationBlendConfig = state.Clips.FirstOrDefault(a => (int)a.ParamValue == animationType);
        return animationBlendConfig == null ? 0 : animationBlendConfig.Clips.Length;
    }
}
