using UnityEngine;

[CreateAssetMenu(fileName = "CharacterPersonalityData", menuName = "Scriptable Objects/CharacterPersonalityData")]
public class CharacterPersonalityData : ScriptableObject
{
    [field: Header("Name")]
    [field: SerializeField] public NamingSettings NamingSettings { get; private set; }
    
    [field: Header("Appearance")]
    [field: SerializeField] public CharacterSkinDataSettings CharacterSkinDataSettings { get; private set; }
    
    [field: Header("Items")]
    [field: SerializeField] public WeaponDataSettings WeaponDataSettings { get; private set; }
    [field: SerializeField] public ArmorDataSettings ArmorDataSettings { get; private set; }
    [field: SerializeField] public InventoryDataSettings InventoryDataSettings { get; private set; }
    
    [field: Header("Character Parameters")]
    [field: SerializeField] public HealthDataSettings HealthDataSettings { get; private set; }
}

[System.Serializable]
public struct NamingSettings
{
    [field: SerializeField] public string CharacterName { get; private set; }
}

[System.Serializable]
public struct CharacterSkinDataSettings
{
    [field: SerializeField] public CharacterSkinData PrimarySkin { get; private set; }
}

[System.Serializable]
public struct WeaponDataSettings
{
    [field: SerializeField] public WeaponData[] EquippedWeapons { get; private set; }
}

[System.Serializable]
public struct ArmorDataSettings
{
    [field: SerializeField] public ArmorData[] EquippedArmor { get; private set; }
}

[System.Serializable]
public struct InventoryDataSettings
{
    [field: SerializeField] public IItemData[] Items { get; private set; }
}


[System.Serializable]
public struct HealthDataSettings
{
    [field: SerializeField] public float MaxHealth { get; private set; }
    [field: SerializeField] public float CurrentHealthPercentage { get; private set; }
    [field: SerializeField] [Range(0, 100)] public float HitReactionTriggerValuePercentage { get; private set; }
}
