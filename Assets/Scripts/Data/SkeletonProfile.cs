using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skeleton Profile")]
public class SkeletonProfile : ScriptableObject
{
    [System.Serializable]
    public struct BoneCorrection
    {
        public HumanBodyBones bone;
        public Vector3 position;
        public Vector3 rotation;
        public float scale;
    }
    
    [System.Serializable]
    public struct BoneRotationOffset
    {
        public HumanBodyBones bone;
        public Vector3 eulerOffset;
    }

    public BoneCorrection[] attachmentCorrections;
    public BoneRotationOffset[] lookAtOffsets; 

    public bool TryGetBoneCorrection(HumanBodyBones bone, out BoneCorrection correction)
    {
        foreach (var c in attachmentCorrections)
        {
            if (c.bone == bone) { correction = c; return true; }
        }
        correction = default;
        return false;
    }
}