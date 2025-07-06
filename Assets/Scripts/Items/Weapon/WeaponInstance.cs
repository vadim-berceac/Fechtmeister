using ModestTree;
using UnityEngine;

public class WeaponInstance : IItemInstance
{
    public IItemData ItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public WeaponDamageComponent DamageComponent { get; set; }
    public Transform[] ItemDecorations { get; set; }

    public WeaponInstance(ref IItemData itemData, CharacterBonesContainer characterBonesContainer)
    {
        ItemData = itemData;
        itemData = null;
        CharacterBonesContainer = characterBonesContainer;
        
        CreateInstance();
        CreateDecorations();
    }
    
    public void CreateInstance()
    {
        if (ItemData.EquippedModelPrefab == null)
        {
            return;
        }
        Instance = Object.Instantiate(ItemData.EquippedModelPrefab).transform;
        
        IKBoneTransform = TryToFindIKBoneTransform();

        DamageComponent = Instance.gameObject.AddComponent<WeaponDamageComponent>();
        
        AttachToBone(Instance, ItemData.BoneData[1]);
    }

    public void CreateDecorations()
    {
        if (ItemData.ItemDecorationData == null)
        {
            return;
        }
        
        ItemDecorations = new Transform[ItemData.ItemDecorationData.Length];

        foreach (var decoration in ItemData.ItemDecorationData)
        {
            ItemDecorations[ItemData.ItemDecorationData.IndexOf(decoration)] = Object.Instantiate(decoration.ItemPrefab).transform;
            AttachToBone(ItemDecorations[ItemData.ItemDecorationData.IndexOf(decoration)], decoration.BoneData);
        }
    }

    public void AttachToBone(Transform instance, BoneData boneData)
    {
        if (boneData == null)
        {
            return;
        }

        var boneTransform = CharacterBonesContainer.GetBoneTransform(boneData.BonesType);
        
        instance.parent = boneTransform.Transform;
        
        instance.SetLocalPositionAndRotation(boneData.Position, boneData.Rotation);
        
        instance.localScale = boneData.Scale;
    }

    public Transform TryToFindIKBoneTransform()
    {
        return ItemData.IKBoneData.IKBoneName.IsEmpty()? null : Instance.FindChildRecursive(ItemData.IKBoneData.IKBoneName);
    }

    public void DestroyInstance()
    {
        ItemData = null;
        CharacterBonesContainer = null;
        if (Instance == null)
        {
            return;
        }
        Instance.parent = null;
        Object.Destroy(Instance.gameObject);

        if (ItemDecorations == null || ItemDecorations.Length == 0)
        {
            return;
        }

        foreach (var decoration in ItemDecorations)
        {
            Object.Destroy(decoration.gameObject);
        }
        ItemDecorations = null;
    }
}
