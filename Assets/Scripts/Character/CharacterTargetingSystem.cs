
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

    public float GetVerticalAngleItem()
    {
        return _itemTargeting.GetVerticalAngleToFirstTarget();
    }

    public float GetHorizontalAngleItem()
    {
        return _itemTargeting.GetHorizontalAngleToFirstTarget();
    }

    public float GetVerticalAngleCharacter()
    {
        return _characterTargeting.GetVerticalAngleToFirstTarget();
    }

    public float GetHorizontalAngleCharacter()
    {
        return _characterTargeting.GetHorizontalAngleToFirstTarget();
    }
}
