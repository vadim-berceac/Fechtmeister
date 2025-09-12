using System;

public class WeaponSystem : IItemInstancesContainer
{
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public IItemInstance InstanceInHands {get; private set;}
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IItemData> OnItemUnEquipped { get; set; }
    public bool WeaponInstanceIsRanged { get; private set; }
    private readonly CharacterCore _characterCore;

    public WeaponSystem(int instancesCount, CharacterCore characterCore)
    {
        InstancesCount = instancesCount;
        _characterCore = characterCore;
        CharacterBonesContainer = _characterCore.BonesContainer;
        Instances = new IItemInstance[instancesCount];

        _characterCore.CharacterInputHandler.OnWeaponSwitch += OnWeaponInstanceSwitched;
    }

    public void SelectWeapon(int itemIndex)
    {
        if (Instances[itemIndex] == null || itemIndex < 0 || itemIndex >= InstancesCount)
        {
            return;
        }
        InstanceInHands = Instances[itemIndex];
        WeaponInstanceIsRanged = ((WeaponData)InstanceInHands.ItemData).IsRanged;
    }

    private void OnWeaponInstanceSwitched(int itemIndex)
    {
        if (!_characterCore.CurrentState.AllowSwitchWeaponInstance)
        {
            return;
        }
        
        SelectWeapon(itemIndex);
    }

    public void AllowAttack(bool value)
    {
        if (InstanceInHands?.ItemControlComponent == null)
        {
            return;
        }
        InstanceInHands.ItemControlComponent.AllowToUse(value);
    }

    public void Destroy()
    {
        _characterCore.CharacterInputHandler.OnWeaponSwitch -= OnWeaponInstanceSwitched;
    }
}
