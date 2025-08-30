
using UnityEngine;

public class CharacterTargetingSystem
{
    public readonly ItemTargeting ItemTargeting;
    public readonly CharacterTargeting CharacterTargeting;
    
    public Transform LastItemTransform { get; private set; }
    public Transform LastCharacterTransform { get; private set; }

    public CharacterTargetingSystem(ItemTargeting itemTargeting, CharacterTargeting characterTargeting)
    {
        ItemTargeting = itemTargeting;
        CharacterTargeting = characterTargeting;
    }

    public void AllowItemTargeting(bool allow)
    {
        ItemTargeting.Allow(allow);
    }

    public void AllowCharacterTargeting(bool allow)
    {
        CharacterTargeting.Allow(allow);
    }

    public float GetVerticalAngle(TargetingMode targetingMode)
    {
        switch (targetingMode)
        {
            case TargetingMode.Item:
                return ItemTargeting.GetVerticalAngleToFirstTarget();
            
            case TargetingMode.Character:
                return CharacterTargeting.GetVerticalAngleToFirstTarget();
            
            default:
                return 0;
        }
    }

    public float GetHorizontalAngle(TargetingMode targetingMode)
    {
        switch (targetingMode)
        {
            case TargetingMode.Item:
                return ItemTargeting.GetHorizontalAngleToFirstTarget();
            
            case TargetingMode.Character:
                return CharacterTargeting.GetHorizontalAngleToFirstTarget();
            
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
            
            case TargetingMode.Character:
                LastCharacterTransform = CharacterTargeting.GetFirstTarget();
                return LastCharacterTransform != null;
            
            default:
                return false;
        }
    }

    public IItemData GetTargetItem()
    {
        if (LastItemTransform == null)
        {
            return null;
        }
        
        LastItemTransform.TryGetComponent<PickupItem>(out var item);

        if (item == null)
        {
            LastItemTransform = null;
            return null;
        }
        
        ItemTargeting.RemoveTarget(LastItemTransform);
        LastItemTransform.gameObject.SetActive(false);
        LastItemTransform = null;

        return item.ItemData;
    }
}

public enum TargetingMode
{
    Item,
    Character
}
