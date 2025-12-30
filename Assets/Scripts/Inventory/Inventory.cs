public class Inventory
{
    public bool IsWeaponOn { get; private set; }
    private readonly CharacterPresetLoader _characterPresetLoader;
    public readonly WeaponSystem WeaponSystem;
    public readonly ArmorSystem ArmorSystem;
    public readonly InventoryBag InventoryBag;
    private readonly CharacterCore _characterCore;

    public Inventory(CharacterCore characterCore, CharacterPresetLoader characterPresetLoader, int weaponSystemInstancesCount)
    {
        _characterPresetLoader = characterPresetLoader;
        _characterCore = characterCore;
        WeaponSystem = new WeaponSystem(weaponSystemInstancesCount, characterCore);

        ArmorSystem = new ArmorSystem(5, characterCore);
        
        InventoryBag = new InventoryBag(96);
        
        InitEquipment();
    }

    public void AddToInventoryBag(IEquppiedItemData equppiedItemData)
    {
        InventoryBag.AddItem(equppiedItemData);
    }

    public void SelectWeaponInstance(int index)
    {
        WeaponSystem.SelectWeapon(index);
    }

    public void WeaponOn()
    {
        IsWeaponOn = true;
        var boneData = WeaponSystem.InstanceInHands.EquppiedItemData.BoneData[0];
        WeaponSystem.InstanceInHands.Animator.AttachToBone(WeaponSystem.InstanceInHands.Instance, boneData.BonesType, boneData.Position, 
            boneData.Rotation.eulerAngles, boneData.Scale, boneData.Active);
    }

    public void WeaponOff()
    {
        IsWeaponOn = false;
        var boneData = WeaponSystem.InstanceInHands.EquppiedItemData.BoneData[1];
        WeaponSystem.InstanceInHands.Animator.AttachToBone(WeaponSystem.InstanceInHands.Instance, boneData.BonesType, boneData.Position, 
            boneData.Rotation.eulerAngles, boneData.Scale, boneData.Active);
    }

    private void InitEquipment()
    {
        if (_characterPresetLoader.CharacterPersonalityData.WeaponDataSettings.EquippedWeapons != null)
        {
            foreach (var w in _characterPresetLoader.CharacterPersonalityData.WeaponDataSettings.EquippedWeapons)
            {
                if (w == null)
                {
                    continue;
                }
                WeaponSystem.Equip(w, _characterCore.LocomotionSettings.CharacterCollider, _characterCore.GraphCore.CoreData.Animator);
            }
            SelectWeaponInstance(0);
        }

        if (_characterPresetLoader.CharacterPersonalityData.ArmorDataSettings.EquippedArmor != null)
        {
            foreach (var a in _characterPresetLoader.CharacterPersonalityData.ArmorDataSettings.EquippedArmor)
            {
                if (a == null)
                {
                    continue;
                }
                ArmorSystem.Equip(a, _characterCore.LocomotionSettings.CharacterCollider, _characterCore.GraphCore.CoreData.Animator);
            }
        }
    }
    
    public void Destroy()
    {
        WeaponSystem.Destroy();
    }
}
