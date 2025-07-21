using System;
using System.Linq;
using ModestTree;

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
    
    public void Equip(IItemData item)
    {
        var emptyInstance = GetEmptyInstance();

        if (emptyInstance >= 0)
        {
            Instances[emptyInstance] = new WeaponInstance(ref item, CharacterBonesContainer);
            return;
        }
        
        OnItemUnEquipped?.Invoke(Instances[0].ItemData);
        DestroyInstance(0);
        Instances[0] = new WeaponInstance(ref item, CharacterBonesContainer);
    }

    public void SelectInstance(int itemIndex)
    {
        if (Instances[itemIndex] == null || itemIndex < 0 || itemIndex >= InstancesCount)
        {
            return;
        }
        InstanceInHands = Instances[itemIndex];
        WeaponInstanceIsRanged = ((WeaponData)InstanceInHands.ItemData).IsRanged;
    }

    public int GetEmptyInstance()
    {
        return Instances.IndexOf(Instances.FirstOrDefault(x => x == null));
    }

    public void DestroyInstance(int index)
    {
        Instances[index].DestroyInstance();
        Instances[index] = null;
    }
}
