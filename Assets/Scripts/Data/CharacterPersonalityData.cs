using UnityEngine;

[CreateAssetMenu(fileName = "CharacterPersonalityData", menuName = "Scriptable Objects/CharacterPersonalityData")]
public class CharacterPersonalityData : ScriptableObject
{
    [field: Header("Skin")]
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public CharacterSkinData CharacterSkinData { get; private set; }
    
    [field: Header("Weapons")]
    [field: SerializeField] public WeaponData[] EquippedWeapons { get; private set; }
    
    [field: Header("Armor")]
    [field: SerializeField] public ArmorData[] EquippedArmor { get; private set; }
    
    [field: Header("Inventory")]
    [field: SerializeField] public IItemData[] Items { get; private set; }
    
    // [field: Header("Health")]
    // [field: SerializeField] public float MaxHealth { get; private set; }
    //
    // [field: Header("Weapon")]
    // [field: SerializeField] public WeaponData[] WeaponData { get; private set; } = new WeaponData [3];
    //
    // [field: Header("Test Armor")]
    // [field: SerializeField] public CharacterSkinData PrimaryArmorData { get; private set; }
    
    //[field: Header("Inventory")]
}
