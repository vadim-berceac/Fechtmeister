
using UnityEngine;

public static class AnimatorExtensions
{
    /// <summary>
    /// Use in LateUpdate!
    /// <returns></returns>
    public static bool AttachTransformSource(this Animator animator, Transform source,
        HumanBodyBones parentBone, Vector3 position, Vector3 rotation, float scale, bool enabled, bool useBone = true)
    {
        if (animator == null || source == null)
            return false;
        
        var originalCullingMode = animator.cullingMode;
    
        if (animator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
        {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        var bone = useBone ? animator.GetBoneTransform(parentBone) : animator.transform;
        if (bone == null)
        {
            animator.cullingMode = originalCullingMode;
            return false;
        }

        source.SetParent(bone, false);
        source.SetLocalPositionAndRotation(position, Quaternion.Euler(rotation));

        var desiredLossy = Vector3.one * scale;
        var parentLossy = bone.lossyScale;

        source.localScale = new Vector3(
            desiredLossy.x / parentLossy.x,
            desiredLossy.y / parentLossy.y,
            desiredLossy.z / parentLossy.z
        );

        source.gameObject.SetActive(enabled);

        animator.cullingMode = originalCullingMode;

        return true;
    }
}
