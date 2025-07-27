
using UnityEngine;

public class CharacterTargetingSystem
{
    private readonly Targeting _itemTargeting;
    private readonly Targeting _characterTargeting;
    
    public Transform LastItemTransform { get; private set; }
    public Transform LastCharacterTransform { get; private set; }

    public CharacterTargetingSystem(Targeting itemTargeting, Targeting characterTargeting)
    {
        _itemTargeting = itemTargeting;
        _characterTargeting = characterTargeting;
    }

    public void AllowItemTargeting(bool allow)
    {
        _itemTargeting.Allow(allow);
    }

    public void AllowCharacterTargeting(bool allow)
    {
        _characterTargeting.Allow(allow);
    }

    public float GetVerticalAngle(TargetingMode targetingMode)
    {
        switch (targetingMode)
        {
            case TargetingMode.Item:
                return _itemTargeting.GetVerticalAngleToFirstTarget();
            
            case TargetingMode.Character:
                return _characterTargeting.GetVerticalAngleToFirstTarget();
            
            default:
                return 0;
        }
    }

    public float GetHorizontalAngle(TargetingMode targetingMode)
    {
        switch (targetingMode)
        {
            case TargetingMode.Item:
                return _itemTargeting.GetHorizontalAngleToFirstTarget();
            
            case TargetingMode.Character:
                return _characterTargeting.GetHorizontalAngleToFirstTarget();
            
            default:
                return 0;
        }
    }

    public bool HasTarget(TargetingMode targetingMode)
    {
        switch (targetingMode)
        {
            case TargetingMode.Item:
                LastItemTransform = _itemTargeting.GetFirstTarget();
                return LastItemTransform != null;
            
            case TargetingMode.Character:
                LastCharacterTransform = _characterTargeting.GetFirstTarget();
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
        
        _itemTargeting.RemoveTarget(LastItemTransform);
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
