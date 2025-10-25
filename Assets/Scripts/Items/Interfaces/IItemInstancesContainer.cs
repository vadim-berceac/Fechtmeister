using System;
using System.Linq;
using UnityEngine;

public interface IItemInstancesContainer
{
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IItemData> OnItemUnEquipped { get; set; }
    public StateTimer StateTimer { get; set; }
}

public static class ItemInstancesContainerExtensions
{
    public static bool Equip(this IItemInstancesContainer container, IItemData item, Collider owner)
    {
        if (ContainsInstance(container, item) || OccupiedSamePosition(container, item))
        {
            return false;
        }
        
        var emptyInstance = GetEmptyInstance(container);

        if (emptyInstance >= 0)
        {
            if (item is WeaponData)
            {
                container.Instances[emptyInstance] = new WeaponInstance(ref item, container.CharacterBonesContainer, owner, container.StateTimer);
            }

            if (item is ArmorData)
            {
                container.Instances[emptyInstance] = new ArmorInstance(ref item, container.CharacterBonesContainer, owner);
            }
        }

        return true;
    }
    
    private static bool ContainsInstance(this IItemInstancesContainer container, IItemData data)
    {
        var instance = container.Instances.FirstOrDefault(i => i != null && i.ItemData == data);
        
        return instance != null;
    }

    private static bool OccupiedSamePosition(this IItemInstancesContainer container, IItemData data)
    {
        var instance = container.Instances.FirstOrDefault(i => i != null && i.ItemData != null && i.ItemData.ItemPosition == data.ItemPosition);
        return instance != null;
    }

    private static int GetEmptyInstance(this IItemInstancesContainer container)
    {
        if (container?.Instances == null)
        {
            return -1; 
        }

        for (var i = 0; i < container.Instances.Length; i++)
        {
            var instance = container.Instances[i];
            if (instance != null && instance.ItemData == null)
            {
                return i;
            }
        }
       
        for (var i = 0; i < container.Instances.Length; i++)
        {
            if (container.Instances[i] == null)
            {
                return i;
            }
        }

        return -1; 
    }

    public static void DestroyInstance(this IItemInstancesContainer container, IItemData data)
    {
        var dataInstance = container.Instances.FirstOrDefault(x => x.ItemData == data);

        if (dataInstance == null)
        {
            return;
        }
        
        dataInstance.DestroyInstance();
    }
}