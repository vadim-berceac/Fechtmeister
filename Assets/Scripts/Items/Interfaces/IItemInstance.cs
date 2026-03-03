using ModestTree;
using UnityEngine;

public interface IItemInstance
{
    public IEquppiedItemData EquppiedItemData { get; set; }
    public Transform Instance { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }
    public PlayableGraphCore PlayableGraphCore { get; set; }
    
    public void CreateInstance();
}

public static class IItemInstanceExtensions
{
    public static void CreateDecorations(this IItemInstance itemInstance)
    {
        if (itemInstance.EquppiedItemData.ItemDecorationData == null)
        {
            return;
        }
        
        itemInstance.ItemDecorations = new Transform[itemInstance.EquppiedItemData.ItemDecorationData.Length];

        foreach (var decoration in itemInstance.EquppiedItemData.ItemDecorationData)
        {
            var part = Object.Instantiate(decoration.ItemPrefab).transform;
            var index = itemInstance.EquppiedItemData.ItemDecorationData.IndexOf(decoration);
            var boneData = itemInstance.EquppiedItemData.ItemDecorationData[index].BoneData;
            itemInstance.ItemDecorations[index] = part;
            itemInstance.PlayableGraphCore.AttachEquipment(part, boneData.BonesType, boneData.Active, boneData.Position, 
                boneData.Rotation.eulerAngles, boneData.Scale, boneData.UseBone);
        }
    }
    
    public static void DestroyInstance(this IItemInstance itemInstance)
    {
        itemInstance.EquppiedItemData = null;
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
}


