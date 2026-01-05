
using UnityEngine;

public class CharacterTargetingSystem
{
    public readonly ItemTargeting ItemTargeting;
    
    public Transform LastItemTransform { get; private set; }

    public CharacterTargetingSystem(ItemTargeting itemTargeting)
    {
        ItemTargeting = itemTargeting;
    }

    public void AllowItemTargeting(bool allow)
    {
        ItemTargeting.Allow(allow);
    }
    
    public float GetVerticalAngle(TargetingMode targetingMode)
    {
        switch (targetingMode)
        {
            case TargetingMode.Item:
                return ItemTargeting.GetVerticalAngleToFirstTarget();
            default:
                return 0;
        }
    }
    
    public bool HasTarget(TargetingMode targetingMode)
    {
        switch (targetingMode)
        {
            case TargetingMode.Item:
                LastItemTransform = ItemTargeting.GetFirstTarget();
                return LastItemTransform != null;
            default:
                return false;
        }
    }

    public (ISimpleItemData data, int amount) GetTargetItem()
    {
        if (LastItemTransform == null)
        {
            return (null, 0);
        }
        
        LastItemTransform.TryGetComponent<PickupItem>(out var item);

        if (item == null)
        {
            LastItemTransform = null;
            return (null, 0);
        }
        
        ItemTargeting.RemoveTarget(LastItemTransform);
        LastItemTransform.gameObject.SetActive(false);
        LastItemTransform = null;

        return item.GetItemData();
    }
}

public enum TargetingMode
{
    Item,
}
