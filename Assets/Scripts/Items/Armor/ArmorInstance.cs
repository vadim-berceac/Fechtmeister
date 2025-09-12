using UnityEngine;

public class ArmorInstance : IItemInstance
{
    public IItemData ItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }

    private Collider _owner;

    public ArmorInstance(ref IItemData itemData, CharacterBonesContainer characterBonesContainer, Collider owner)
    {
        ItemData = itemData;
        itemData = null;
        CharacterBonesContainer = characterBonesContainer;
        _owner = owner;

        CreateInstance();
        this.CreateDecorations();
    }
    
    public void CreateInstance()
    {
        if (ItemData.EquippedModelPrefab == null)
        {
            return;
        }

        if (ItemData.BoneData == null || ItemData.BoneData.Length < 1 || ItemData.BoneData[0] == null)
        {
            return;
        }

        CreateSkinnedMesh();
    }

    private void CreateSkinnedMesh()
    {
        Debug.Log("Ищем SkinnedMesh в" + ItemData.EquippedModelPrefab.name);
    }
}
