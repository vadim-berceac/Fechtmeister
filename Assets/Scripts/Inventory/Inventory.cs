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

        ArmorSystem = new ArmorSystem(5, characterCore);
        
        ProjectileSystem = new ProjectileSystem();
        
        InventoryBag = new InventoryBag(96);
        
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

    public void WeaponOn()
    {
        WeaponSystem.InstanceInHands.AttachToBone(WeaponSystem.InstanceInHands.Instance, WeaponSystem.InstanceInHands.ItemData.BoneData[0]);
    }

    public void WeaponOff()
    {
        WeaponSystem.InstanceInHands.AttachToBone(WeaponSystem.InstanceInHands.Instance, WeaponSystem.InstanceInHands.ItemData.BoneData[1]);
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

        if (_characterPresetLoader.CharacterPersonalityData.EquippedArmor != null)
        {
            foreach (var a in _characterPresetLoader.CharacterPersonalityData.EquippedArmor)
            {
                if (a == null)
                {
                    continue;
                }
                ArmorSystem.Equip(a);
            }
        }
    }
    
    public void Destroy()
    {
        WeaponSystem.Destroy();
    }
}
