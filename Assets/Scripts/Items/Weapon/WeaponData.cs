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
    [field: SerializeField] public RangeTypes RangeType { get; set; }
    [field: SerializeField] public int AnimationType { get; set; }
    [field: SerializeField] public AttackCounterSettings AttackCounterSettings { get; set; }
    [field: SerializeField] public BoneData[] BoneData { get; set; }
    [field: SerializeField] public ItemDecorationData[] ItemDecorationData { get; set; }
}

[System.Serializable]
public struct WeaponParams
{
    [field: SerializeField] public float Damage { get; set; }
    [field: SerializeField] public DamageTypes DamageType { get; set; }
    [field: SerializeField] public float AttackSpeed { get; set; }
    [field: SerializeField] public float PreferredDistance { get; set; }
    [field: SerializeField] public Vector3 HitBoxSize { get; set; }
    [field: SerializeField] public float HitBoxForwardOffset { get; set; }
    [field: SerializeField] public SfxSet StartSounds { get; set; }
    [field: SerializeField] public SfxSet WhooshSounds { get; set; }
    [field: SerializeField] public SfxSet HitSounds { get; set; }
    [field: SerializeField] public GameObject StartParticlePrefab { get; set; }
    [field: SerializeField] public GameObject HitParticlePrefab { get; set; }
    [field: SerializeField] public WastingChargesSettings WastingCharges { get; set; }
}

[System.Serializable]
public struct WastingChargesSettings
{
    [field: SerializeField] public WastingCharges WastingCharges { get; set; }
    [field: SerializeField] public int ChargesPerUse {get; set;}
}

public enum WastingCharges
{
    None,
    Projectiles,
    Mana,
    Health
}

public enum RangeTypes
{
    Melee = 0,
    Ranged = 1,
    Mixed = 2
}
