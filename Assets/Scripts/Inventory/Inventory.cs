public class Inventory
{
    private readonly CharacterPresetLoader _characterPresetLoader;
    public readonly WeaponSystem WeaponSystem;
    public readonly ArmorSystem ArmorSystem;
    public readonly ProjectileSystem ProjectileSystem;
    public readonly InventoryBag InventoryBag;
    private readonly CharacterCore _characterCore;

    public Inventory(CharacterCore characterCore, CharacterPresetLoader characterPresetLoader, int weaponSystemInstancesCount)
    {
        _characterPresetLoader = characterPresetLoader;
        WeaponSystem = new WeaponSystem(weaponSystemInstancesCount, characterCore);
        WeaponSystem.OnItemUnEquipped += OnItemUnEquipped;

        ArmorSystem = new ArmorSystem();
        ArmorSystem.OnItemUnEquipped += OnItemUnEquipped;
        
        ProjectileSystem = new ProjectileSystem();
        
        InventoryBag = new InventoryBag(25);
        
        InitEquipment();
    }

    public void AddToInventoryBag(IItemData itemData)
    {
        InventoryBag.AddItem(itemData);
    }

    public void SelectWeaponInstance(int index)
    {
        WeaponSystem.SelectWeapon(index);
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        WeaponSystem.Equip(weaponData);
    }

    public void WeaponOn()
    {
        WeaponSystem.InstanceInHands.AttachToBone(WeaponSystem.InstanceInHands.Instance, WeaponSystem.InstanceInHands.ItemData.BoneData[0]);
    }

    public void WeaponOff()
    {
        WeaponSystem.InstanceInHands.AttachToBone(WeaponSystem.InstanceInHands.Instance, WeaponSystem.InstanceInHands.ItemData.BoneData[1]);
    }

    public void EquipArmor(ArmorData armorData)
    {
        
    }

    private void InitEquipment()
    {
        if (_characterPresetLoader.CharacterPersonalityData.EquippedWeapons != null)
        {
            foreach (var w in _characterPresetLoader.CharacterPersonalityData.EquippedWeapons)
            {
                if (w == null)
                {
                    continue;
                }
                WeaponSystem.Equip(w);
            }
            SelectWeaponInstance(0);
        }
    }

    private void OnItemUnEquipped(IItemData weaponData)
    {
        // переносим дату в  InventoryBag
    }

    public void Destroy()
    {
        WeaponSystem.OnItemUnEquipped -= OnItemUnEquipped;
        ArmorSystem.OnItemUnEquipped -= OnItemUnEquipped;
        WeaponSystem.Destroy();
    }
}
