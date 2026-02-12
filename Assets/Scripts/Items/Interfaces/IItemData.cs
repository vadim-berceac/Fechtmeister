using UnityEngine;

public interface IEquppiedItemData : ISimpleItemData
{
    public ItemsPositions.Occupied ItemPosition { get; set; }
    public BoneData[] BoneData { get; set; }
    public ItemDecorationData[] ItemDecorationData { get; set; }
}

public interface ISimpleItemData
{
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }
    public Sprite ItemIcon { get; set; }
    public GameObject EquippedModelPrefab { get; set; }
    public GameObject GroundedModelPrefab { get; set; }
}