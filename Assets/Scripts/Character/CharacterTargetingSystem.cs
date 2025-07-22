
public class CharacterTargetingSystem
{
    private readonly Targeting _itemTargeting;
    private readonly Targeting _characterTargeting;

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
}

public enum TargetingMode
{
    Item,
    Character
}
