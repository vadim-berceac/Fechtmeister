using System;
using System.Linq;
using UnityEngine;

public interface IItemInstancesContainer
{
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IEquppiedItemData> OnItemUnEquipped { get; set; }
    public StateTimer StateTimer { get; set; }
}

public static class ItemInstancesContainerExtensions
{
    public static bool Equip(this IItemInstancesContainer container, IEquppiedItemData equppiedItem, Collider owner)
    {
        if (ContainsInstance(container, equppiedItem) || OccupiedSamePosition(container, equppiedItem))
        {
            return false;
        }
        
        var emptyInstance = GetEmptyInstance(container);

        if (emptyInstance >= 0)
        {
            if (equppiedItem is WeaponData)
            {
                container.Instances[emptyInstance] = new WeaponInstance(ref equppiedItem, container.CharacterBonesContainer, owner,
                    ((WeaponSystem)container).CharacterCore.SceneCharacterContainer);
            }

            if (equppiedItem is ArmorData)
            {
                container.Instances[emptyInstance] = new ArmorInstance(ref equppiedItem, container.CharacterBonesContainer, owner);
            }
        }

        return true;
    }
    
    private static bool ContainsInstance(this IItemInstancesContainer container, IEquppiedItemData data)
    {
        var instance = container.Instances.FirstOrDefault(i => i != null && i.EquppiedItemData == data);
        
        return instance != null;
    }

    private static bool OccupiedSamePosition(this IItemInstancesContainer container, IEquppiedItemData data)
    {
        var instance = container.Instances.FirstOrDefault(i => i != null && i.EquppiedItemData != null && i.EquppiedItemData.ItemPosition == data.ItemPosition);
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
            if (instance != null && instance.EquppiedItemData == null)
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

    public static void DestroyInstance(this IItemInstancesContainer container, IEquppiedItemData data)
    {
        var dataInstance = container.Instances.FirstOrDefault(x => x.EquppiedItemData == data);

        if (dataInstance == null)
        {
            return;
        }
        
        dataInstance.DestroyInstance();
    }
}