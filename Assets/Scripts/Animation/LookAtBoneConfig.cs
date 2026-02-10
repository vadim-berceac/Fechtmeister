using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

[System.Serializable]
public struct BoneLookAtData
{
    public TransformStreamHandle boneHandle;
    public NativeArray<Vector3> targetPosition;
    
    [Range(0f, 1f)]
    public float weight;
    
    public Vector3 aimAxis;
    public Vector3 upAxis;
    
    [Range(0f, 180f)]
    public float maxAngle;
    
    [Range(-180f, 180f)]
    public float minVerticalAngle;
    
    [Range(-180f, 180f)]
    public float maxVerticalAngle;
    
    [Range(-180f, 180f)]
    public float minHorizontalAngle;
    
    [Range(-180f, 180f)]
    public float maxHorizontalAngle;
}

[System.Serializable]
public class LookAtBoneConfig
{
    [Header("Bone Setup")]
    public HumanBodyBones humanBone;
    public Transform target;
    
    [Range(0f, 1f)]
    public float weight = 1f;
    
    [Header("Axes")]
    public Vector3 aimAxis = Vector3.forward;
    public Vector3 upAxis = Vector3.up;
    
    [Header("Angle Limits")]
    [Range(0f, 180f)]
    public float maxAngle = 90f;
    
    [Header("Advanced Constraints")]
    public bool useAdvancedConstraints = false;
    
    [Range(-90f, 90f)]
    public float minVerticalAngle = -45f;
    
    [Range(-90f, 90f)]
    public float maxVerticalAngle = 45f;
    
    [Range(-180f, 180f)]
    public float minHorizontalAngle = -60f;
    
    [Range(-180f, 180f)]
    public float maxHorizontalAngle = 60f;
    
    [HideInInspector]
    public Transform bone;
    
    [HideInInspector]
    public NativeArray<Vector3> targetPosArray;
}

public struct MultiLookAtJob : IAnimationJob
{
    public NativeArray<BoneLookAtData> bones;
    
    public void ProcessRootMotion(AnimationStream stream) { }
    
    public void ProcessAnimation(AnimationStream stream)
    {
        for (int i = 0; i < bones.Length; i++)
        {
            var bone = bones[i];
            
            if (bone.weight <= 0f || !bone.targetPosition.IsCreated || bone.targetPosition.Length == 0)
                continue;
            
            ProcessBone(stream, bone);
        }
    }
    
    private void ProcessBone(AnimationStream stream, BoneLookAtData bone)
    {
        Vector3 bonePos = bone.boneHandle.GetPosition(stream);
        Vector3 direction = bone.targetPosition[0] - bonePos;
        
        if (direction.sqrMagnitude < 0.001f) return;
        
        Quaternion currentRot = bone.boneHandle.GetRotation(stream);
        Quaternion targetRot = Quaternion.LookRotation(direction, bone.upAxis);
        
        if (bone.aimAxis != Vector3.forward)
        {
            Quaternion offset = Quaternion.FromToRotation(bone.aimAxis, Vector3.forward);
            targetRot *= Quaternion.Inverse(offset);
        }
        
        targetRot = ApplyConstraints(currentRot, targetRot, bone);
        
        bone.boneHandle.SetRotation(stream, Quaternion.Slerp(currentRot, targetRot, bone.weight));
    }
    
    private Quaternion ApplyConstraints(Quaternion current, Quaternion target, BoneLookAtData bone)
    {
        if (bone.maxAngle > 0f && bone.maxAngle < 180f)
        {
            float angle = Quaternion.Angle(current, target);
            if (angle > bone.maxAngle)
            {
                target = Quaternion.RotateTowards(current, target, bone.maxAngle);
            }
        }
        
        if (bone.minVerticalAngle < bone.maxVerticalAngle || 
            bone.minHorizontalAngle < bone.maxHorizontalAngle)
        {
            Vector3 euler = target.eulerAngles;
            
            if (euler.x > 180f) euler.x -= 360f;
            if (euler.y > 180f) euler.y -= 360f;
            
            if (bone.minVerticalAngle < bone.maxVerticalAngle)
            {
                euler.x = Mathf.Clamp(euler.x, bone.minVerticalAngle, bone.maxVerticalAngle);
            }
            
            if (bone.minHorizontalAngle < bone.maxHorizontalAngle)
            {
                euler.y = Mathf.Clamp(euler.y, bone.minHorizontalAngle, bone.maxHorizontalAngle);
            }
            
            target = Quaternion.Euler(euler);
        }
        
        return target;
    }
}