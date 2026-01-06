using UnityEngine;

[System.Serializable]
public struct BoneData
{
    [field: SerializeField] public bool UseBone { get; set; }
    [field: SerializeField] public HumanBodyBones BonesType { get; private set; }
    [field: SerializeField] public Vector3 Position { get; private set; }
    [field: SerializeField] public Quaternion Rotation { get; private set; }
    [field: SerializeField] public float Scale { get; private set; }
    [field: SerializeField] public bool Active { get; private set; }
}

[System.Serializable]
public struct IKBoneData
{
    [field: SerializeField] public string IKBoneName { get; private set; }
    [field: SerializeField] public BoneData CharacterBoneConnected { get; private set; }
}
