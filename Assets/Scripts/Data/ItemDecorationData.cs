using UnityEngine;

[System.Serializable]
public struct ItemDecorationData
{
    [field:SerializeField] public GameObject ItemPrefab { get; set; }
    [field:SerializeField] public BoneData BoneData { get; set; }
}
