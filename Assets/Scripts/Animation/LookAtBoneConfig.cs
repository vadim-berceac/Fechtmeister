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
    
    // Новое: оффсет вращения в локальном пространстве кости
    public Quaternion rotationOffset;
    
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
    
    [Header("Rotation Offset")]
    [Tooltip("Additional rotation offset applied after LookAt (in degrees)")]
    public Vector3 rotationOffsetEuler = Vector3.zero;
    
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
    
    // Кешированный quaternion для оффсета
    [HideInInspector]
    public Quaternion cachedRotationOffset;
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
        
        // Компенсация оси
        if (bone.aimAxis != Vector3.forward)
        {
            Quaternion offset = Quaternion.FromToRotation(bone.aimAxis, Vector3.forward);
            targetRot *= Quaternion.Inverse(offset);
        }
        
        // Применяем оффсет вращения
        targetRot *= bone.rotationOffset;
        
        // Применяем ограничения ОТНОСИТЕЛЬНО текущей ротации
        targetRot = ApplyConstraints(currentRot, targetRot, bone);
        
        bone.boneHandle.SetRotation(stream, Quaternion.Slerp(currentRot, targetRot, bone.weight));
    }
    
    private Quaternion ApplyConstraints(Quaternion current, Quaternion target, BoneLookAtData bone)
    {
        // Общее ограничение угла
        if (bone.maxAngle > 0f && bone.maxAngle < 180f)
        {
            float angle = Quaternion.Angle(current, target);
            if (angle > bone.maxAngle)
            {
                target = Quaternion.RotateTowards(current, target, bone.maxAngle);
            }
        }
        
        // Ограничения по осям
        if (bone.minVerticalAngle < bone.maxVerticalAngle || 
            bone.minHorizontalAngle < bone.maxHorizontalAngle)
        {
            Quaternion relativeRotation = Quaternion.Inverse(current) * target;
            Vector3 euler = relativeRotation.eulerAngles;
            
            // Нормализация от -180 до 180
            if (euler.x > 180f) euler.x -= 360f;
            if (euler.y > 180f) euler.y -= 360f;
            if (euler.z > 180f) euler.z -= 360f;
            
            bool clamped = false;
            
            if (bone.minVerticalAngle < bone.maxVerticalAngle)
            {
                float clampedX = Mathf.Clamp(euler.x, bone.minVerticalAngle, bone.maxVerticalAngle);
                if (Mathf.Abs(clampedX - euler.x) > 0.001f)
                {
                    euler.x = clampedX;
                    clamped = true;
                }
            }
            
            if (bone.minHorizontalAngle < bone.maxHorizontalAngle)
            {
                float clampedY = Mathf.Clamp(euler.y, bone.minHorizontalAngle, bone.maxHorizontalAngle);
                if (Mathf.Abs(clampedY - euler.y) > 0.001f)
                {
                    euler.y = clampedY;
                    clamped = true;
                }
            }
            
            if (clamped)
            {
                relativeRotation = Quaternion.Euler(euler);
                target = current * relativeRotation;
            }
        }
        
        return target;
    }
}