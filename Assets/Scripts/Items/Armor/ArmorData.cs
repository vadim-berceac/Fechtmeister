using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ArmorData", menuName = "Scriptable Objects/ArmorData")]
public class ArmorData : ScriptableObject, IItemData
{
    [field: SerializeField] public string ItemName { get; set; }
    [field: SerializeField] public string ItemDescription { get; set; }
    [field: SerializeField] public Sprite ItemIcon { get; set; }
    [field: SerializeField] public GameObject EquippedModelPrefab { get; set; }
    [field: SerializeField] public GameObject GroundedModelPrefab { get; set; }
    [field: SerializeField] public ItemsPositions.Ocuupied ItemPosition { get; set; }
    [field: SerializeField] public BoneData[] BoneData { get; set; }
    [field: SerializeField] public IKBoneData IKBoneData { get; set; }
    [field: SerializeField] public ItemDecorationData[] ItemDecorationData { get; set; }

    public BoneData GetBoneData(CharacterBones.Type bone)
    {
        return BoneData.FirstOrDefault(b => b.BonesType == bone);
    }
}
