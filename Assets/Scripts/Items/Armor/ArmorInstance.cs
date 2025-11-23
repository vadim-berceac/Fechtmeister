using UnityEngine;

public class ArmorInstance : IItemInstance
{
    public IEquppiedItemData EquppiedItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }

    private Collider _owner;

    public ArmorInstance(ref IEquppiedItemData equppiedItemData, CharacterBonesContainer characterBonesContainer, Collider owner)
    {
        EquppiedItemData = equppiedItemData;
        equppiedItemData = null;
        CharacterBonesContainer = characterBonesContainer;
        _owner = owner;

        CreateInstance();
        this.CreateDecorations();
    }
    
    public void CreateInstance()
    {
        if (EquppiedItemData.EquippedModelPrefab == null)
        {
            return;
        }

        if (EquppiedItemData.BoneData == null || EquppiedItemData.BoneData.Length < 1 || EquppiedItemData.BoneData[0] == null)
        {
            return;
        }

        CreateSkinnedMesh();
    }

    private void CreateSkinnedMesh()
    {
        Debug.Log("Ищем SkinnedMesh в" + EquppiedItemData.EquippedModelPrefab.name);
    }
}
