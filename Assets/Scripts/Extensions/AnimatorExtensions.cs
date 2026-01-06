
using UnityEngine;

public static class AnimatorExtensions
{
    public static void AttachTransformSource(this Animator animator, Transform source,
        HumanBodyBones parentBone, Vector3 position, Vector3 rotation, float scale, bool enabled, bool useBone = true)
    {
        var bone = useBone ? animator.GetBoneTransform(parentBone) : animator.transform;
        if (bone == null || source == null)
            return;

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
    }
}
