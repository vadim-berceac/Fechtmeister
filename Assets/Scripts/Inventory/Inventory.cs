public class Inventory
{
    private readonly WeaponSystem _weaponSystem;
    private readonly ArmorSystem _armorSystem;
    private readonly InventoryBag _inventoryBag;

    public Inventory(CharacterBonesContainer characterBonesContainer, int weaponSystemInstancesCount)
    {
        _weaponSystem = new WeaponSystem(weaponSystemInstancesCount, characterBonesContainer);
        _weaponSystem.OnItemUnEquipped += OnItemUnEquipped;

        _armorSystem = new ArmorSystem();
        _armorSystem.OnItemUnEquipped += OnItemUnEquipped;
    }

    public void SelectWeaponInstance(int index)
    {
        _weaponSystem.SelectInstance(index);
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        _weaponSystem.Equip(weaponData);
    }

    public void WeaponOn()
    {
        _weaponSystem.InstanceInHands.AttachToBone(_weaponSystem.InstanceInHands.Instance, _weaponSystem.InstanceInHands.ItemData.BoneData[0]);
    }

    public void WeaponOff()
    {
        _weaponSystem.InstanceInHands.AttachToBone(_weaponSystem.InstanceInHands.Instance, _weaponSystem.InstanceInHands.ItemData.BoneData[1]);
    }

    public void EquipArmor(ArmorData armorData)
    {
        
    }

    private void OnItemUnEquipped(IItemData weaponData)
    {
        // переносим дату в  InventoryBag
    }

    public void Destroy()
    {
        _weaponSystem.OnItemUnEquipped -= OnItemUnEquipped;
        _armorSystem.OnItemUnEquipped -= OnItemUnEquipped;
    }
}
