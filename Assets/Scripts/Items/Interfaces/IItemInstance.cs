using ModestTree;
using UnityEngine;

public interface IItemInstance
{
    public IEquppiedItemData EquppiedItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }
    
    public void CreateInstance();
}

public static class IItemInstanceExtensions
{
    public static void AttachToBone(this IItemInstance itemInstance, Transform instance, BoneData boneData)
    {
        if (boneData == null)
        {
            return;
        }

        var boneTransform = itemInstance.CharacterBonesContainer.GetBoneTransform(boneData.BonesType);
        
        instance.parent = boneTransform.Transform;
        
        instance.SetLocalPositionAndRotation(boneData.Position, boneData.Rotation);
        
        instance.localScale = boneData.Scale;
    }
    
    public static void CreateDecorations(this IItemInstance itemInstance)
    {
        if (itemInstance.EquppiedItemData.ItemDecorationData == null)
        {
            return;
        }
        
        itemInstance.ItemDecorations = new Transform[itemInstance.EquppiedItemData.ItemDecorationData.Length];

        foreach (var decoration in itemInstance.EquppiedItemData.ItemDecorationData)
        {
            itemInstance.ItemDecorations[itemInstance.EquppiedItemData.ItemDecorationData.IndexOf(decoration)] = Object.Instantiate(decoration.ItemPrefab).transform;
            AttachToBone(itemInstance, itemInstance.ItemDecorations[itemInstance.EquppiedItemData.ItemDecorationData.IndexOf(decoration)], decoration.BoneData);
        }
    }
    
    public static void DestroyInstance(this IItemInstance itemInstance)
    {
        itemInstance.EquppiedItemData = null;
        itemInstance.CharacterBonesContainer = null;
        if (itemInstance.Instance == null)
        {
            return;
        }
        itemInstance.Instance.parent = null;
        Object.Destroy(itemInstance.Instance.gameObject);

        if (itemInstance.ItemDecorations == null || itemInstance.ItemDecorations.Length == 0)
        {
            return;
        }

        foreach (var decoration in itemInstance.ItemDecorations)
        {
            Object.Destroy(decoration.gameObject);
        }
        itemInstance.ItemDecorations = null;
    }

    public static Transform TryToFindIKBoneTransform(this IItemInstance itemInstance)
    {
        return itemInstance.EquppiedItemData.IKBoneData.IKBoneName.IsEmpty()? null : itemInstance.Instance.FindChildRecursive(itemInstance.EquppiedItemData.IKBoneData.IKBoneName);
    }
}


