using System;

public class WeaponSystem : IItemInstancesContainer
{
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public IItemInstance InstanceInHands {get; set;}
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IItemData> OnItemUnEquipped { get; set; }
    public bool WeaponInstanceIsRanged { get; set; }

    public WeaponSystem(int instancesCount, CharacterBonesContainer characterBonesContainer)
    {
        InstancesCount = instancesCount;
        CharacterBonesContainer = characterBonesContainer;
        Instances = new IItemInstance[instancesCount];
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
}
