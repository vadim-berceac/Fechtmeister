using System;

public class WeaponSystem : IItemInstancesContainer
{
    public IItemInstance InstanceInHands {get; private set;}
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IEquppiedItemData> OnItemUnEquipped { get; set; }
    public StateTimer StateTimer { get; set; }
    public bool WeaponInstanceIsRanged { get; private set; }
    public CharacterCore CharacterCore { get; private set; }

    public WeaponSystem(int instancesCount, CharacterCore characterCore)
    {
        InstancesCount = instancesCount;
        CharacterCore = characterCore;
        Instances = new IItemInstance[instancesCount];
        StateTimer = characterCore.StateTimer;

        CharacterCore.CharacterInputHandler.OnWeaponSwitch += OnWeaponInstanceSwitched;
    }

    public void SelectWeapon(int itemIndex)
    {
        if (Instances[itemIndex] == null || itemIndex < 0 || itemIndex >= InstancesCount)
        {
            return;
        }
        InstanceInHands = Instances[itemIndex];
        WeaponInstanceIsRanged = ((WeaponData)InstanceInHands.EquppiedItemData).IsRanged;
    }

    private void OnWeaponInstanceSwitched(int itemIndex)
    {
        if (!CharacterCore.CurrentState.AllowSwitchWeaponInstance)
        {
            return;
        }
        
        SelectWeapon(itemIndex);
    }
    
    public void Destroy()
    {
        CharacterCore.CharacterInputHandler.OnWeaponSwitch -= OnWeaponInstanceSwitched;
    }
}
