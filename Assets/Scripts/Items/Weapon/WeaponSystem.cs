using System;
using ModestTree;

public class WeaponSystem : IItemInstancesContainer
{
    public IItemInstance InstanceInHands {get; private set;}
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IEquppiedItemData> OnItemEquipped { get; set; }
    public Action<IEquppiedItemData> OnItemUnEquipped { get; set; }
    public Action<WeaponData> OnWeaponInHandsSelected { get; set; }
    public StateTimer StateTimer { get; set; }
    public bool WeaponInstanceIsRanged { get; private set; }
    public CharacterCore CharacterCore { get; private set; }
    public bool SelectedInstanceNotEmpty;

    private int _bufferedNewWeaponIndex;

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
        if (Instances[itemIndex] == null || Instances[itemIndex].EquppiedItemData == null 
                                         || itemIndex < 0 || itemIndex >= InstancesCount)
        {
            SelectedInstanceNotEmpty = false;
            return;
        }

        SelectedInstanceNotEmpty = true;
        InstanceInHands = Instances[itemIndex];
        WeaponInstanceIsRanged = ((WeaponData)InstanceInHands.EquppiedItemData).RangeType != RangeTypes.Melee;
        OnWeaponInHandsSelected?.Invoke((WeaponData)InstanceInHands.EquppiedItemData);
    }

    public bool CanDrawWeapon()
    {
        var result = CharacterCore.CharacterInputHandler.IsWeaponDraw && SelectedInstanceNotEmpty;
        return result;
    }

    public bool CanUnDrawWeapon()
    {
        var result = !CharacterCore.CharacterInputHandler.IsWeaponDraw 
                     && _bufferedNewWeaponIndex == Instances.IndexOf(InstanceInHands)
                     && SelectedInstanceNotEmpty;
        return result;
    }

    private void OnWeaponInstanceSwitched(int itemIndex)
    {
        var isNull = Instances[itemIndex] == null;
        var dataIsNull = Instances[itemIndex]?.EquppiedItemData == null;
        var indexInvalid = itemIndex < 0 || itemIndex >= InstancesCount;
    
        if (isNull || dataIsNull || indexInvalid)
        {
            _bufferedNewWeaponIndex = itemIndex;
            SelectedInstanceNotEmpty = false;
            return;
        }
    
        _bufferedNewWeaponIndex = itemIndex;
        SelectedInstanceNotEmpty = true; 
    
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
