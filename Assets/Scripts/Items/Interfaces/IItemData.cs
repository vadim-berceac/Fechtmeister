using UnityEngine;

public interface IItemData
{
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }
    public Sprite ItemIcon { get; set; }
    public GameObject EquippedModelPrefab { get; set; }
    public GameObject GroundedModelPrefab { get; set; }
    public BoneData[] BoneData { get; set; }
    
    public BoneData GetBoneData(CharacterBones.Type bone);
}