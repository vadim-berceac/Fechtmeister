using System;
using System.Linq;
using UnityEngine;

public interface IItemInstancesContainer
{
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IEquppiedItemData> OnItemUnEquipped { get; set; }
    public StateTimer StateTimer { get; set; }
}

public static class ItemInstancesContainerExtensions
{
    public static bool Equip(this IItemInstancesContainer container, IEquppiedItemData equppiedItem, Collider owner, Animator animator)
    {
        if (ContainsInstance(container, equppiedItem) || OccupiedSamePosition(container, equppiedItem))
        {
            return false;
        }
        
        var emptyInstanceIndex = GetEmptyInstance(container);

        if (emptyInstanceIndex < 0)
        {
            return false;
        }
        
        switch (equppiedItem)
        {
            case WeaponData :
                container.Instances[emptyInstanceIndex] = 
                    new WeaponInstance(ref equppiedItem, owner, ((WeaponSystem)container).CharacterCore.SceneCharacterContainer, animator);
                return true;

            case ArmorData :
                container.Instances[emptyInstanceIndex] = 
                    new ArmorInstance(ref equppiedItem, owner, animator);
                return true;
            
            case ProjectileData :
                container.Instances[emptyInstanceIndex] =
                    new ProjectileInstance(ref equppiedItem, owner, animator);
                return true;

            default:
                return false;
        }
    }
    
    private static bool ContainsInstance(this IItemInstancesContainer container, IEquppiedItemData data)
    {
        var instance = container.Instances.FirstOrDefault(i => i != null && i.EquppiedItemData == data);
        
        return instance != null;
    }

    private static bool OccupiedSamePosition(this IItemInstancesContainer container, IEquppiedItemData data)
    {
        var instance = container.Instances.FirstOrDefault(i => i != null && i.EquppiedItemData 
            != null && i.EquppiedItemData.ItemPosition == data.ItemPosition);
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

    public static void DestroyInstance(this IItemInstancesContainer container, ISimpleItemData data)
    {
        var dataInstance = container.Instances.FirstOrDefault(x => x.EquppiedItemData == data);

        if (dataInstance == null)
        {
            Debug.Log($"{data.ItemName} instance not found");
            return;
        }
        
        dataInstance.DestroyInstance();
    }
}