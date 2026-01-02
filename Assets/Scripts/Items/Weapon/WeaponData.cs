using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject, IEquppiedItemData
{
    [field: SerializeField] public WeaponParams WeaponParams { get; set; }
    [field: SerializeField] public string ItemName { get; set; }
    [field: SerializeField] public string ItemDescription { get; set; }
    [field: SerializeField] public Sprite ItemIcon { get; set; }
    [field: SerializeField] public GameObject EquippedModelPrefab { get; set; }
    [field: SerializeField] public GameObject GroundedModelPrefab { get; set; }
    [field: SerializeField] public ItemsPositions.Occupied ItemPosition { get; set; }
    [field: SerializeField] public bool IsRanged { get; set; }
    [field: SerializeField] public int AnimationType { get; set; }
    [field: SerializeField] public AttackCounterSettings AttackCounterSettings { get; set; }
    [field: SerializeField] public BoneData[] BoneData { get; set; }
    [field: SerializeField] public IKBoneData IKBoneData { get; set; }
    [field: SerializeField] public ItemDecorationData[] ItemDecorationData { get; set; }
}

[System.Serializable]
public struct WeaponParams
{
    [field: SerializeField] public float Damage { get; set; }
    [field: SerializeField] public DamageTypes DamageType { get; set; }
    [field: SerializeField] public float AttackSpeed { get; set; }
    [field: SerializeField] public Vector3 HitBoxSize { get; set; }
    [field: SerializeField] public float HitBoxForwardOffset { get; set; }
    [field: SerializeField] public SfxSet WhooshSounds { get; set; }
    [field: SerializeField] public SfxSet HitSounds { get; set; }
}
