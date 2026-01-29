
public class Inventory
{
    public bool IsWeaponOn { get; private set; }
    private readonly CharacterPresetLoader _characterPresetLoader;
    public readonly WeaponSystem WeaponSystem;
    public readonly ArmorSystem ArmorSystem;
    public readonly ProjectileSystem ProjectileSystem;
    public readonly InventoryBag InventoryBag;
    private readonly CharacterCore _characterCore;

    public Inventory(CharacterCore characterCore, CharacterPresetLoader characterPresetLoader, int weaponSystemInstancesCount)
    {
        _characterPresetLoader = characterPresetLoader;
        _characterCore = characterCore;
        WeaponSystem = new WeaponSystem(weaponSystemInstancesCount, characterCore);

        ArmorSystem = new ArmorSystem(5, characterCore);
        ProjectileSystem = new ProjectileSystem(1, characterCore);
        InventoryBag = new InventoryBag(96);
        
        InitEquipment();
    }

    public void AddToInventoryBag(ISimpleItemData data, int amount)
    {
        InventoryBag.AddItem(data, amount);
    }

    public void RemoveFromInventoryBag(ISimpleItemData data, int amount)
    {
        InventoryBag.RemoveItem(data, amount);
    }
    
    public void SelectWeaponInstance(int index)
    {
        WeaponSystem.SelectWeapon(index);
    }

    public void WeaponOn()
    {
        var boneData = WeaponSystem.InstanceInHands.EquppiedItemData.BoneData[0];
        IsWeaponOn = WeaponSystem.InstanceInHands.Animator.AttachTransformSource(WeaponSystem.InstanceInHands.Instance, boneData.BonesType, boneData.Position, 
            boneData.Rotation.eulerAngles, boneData.Scale, boneData.Active, boneData.UseBone);
    }

    public void WeaponOff()
    {
        var boneData = WeaponSystem.InstanceInHands.EquppiedItemData.BoneData[1];
        IsWeaponOn = !WeaponSystem.InstanceInHands.Animator.AttachTransformSource(WeaponSystem.InstanceInHands.Instance, boneData.BonesType, boneData.Position, 
            boneData.Rotation.eulerAngles, boneData.Scale, boneData.Active, boneData.UseBone);
    }

    private void InitEquipment()
    {
        if (_characterPresetLoader.CharacterPersonalityData.WeaponDataSettings.EquippedWeapons.Length > 0)
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

        if (_characterPresetLoader.CharacterPersonalityData.ArmorDataSettings.EquippedArmor.Length > 0)
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

        if (_characterPresetLoader.CharacterPersonalityData.ProjectilesDataSettings.EquippedProjectiles != null)
        {
            var projectileDataSettings = _characterPresetLoader.CharacterPersonalityData.ProjectilesDataSettings;
            ProjectileSystem.Equip(projectileDataSettings.EquippedProjectiles, 
                _characterCore.LocomotionSettings.CharacterCollider, _characterCore.GraphCore.CoreData.Animator);
            ProjectileSystem.AddItem(projectileDataSettings.EquippedProjectiles, projectileDataSettings.AmountOfProjectiles);
        }
    }
    
    public void Destroy()
    {
        WeaponSystem.Destroy();
    }
}
