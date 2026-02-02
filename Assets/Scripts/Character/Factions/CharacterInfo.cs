
public class CharacterInfo
{
    public string Name { get; set; }
    public FactionsEnum FactionType { get; set; }
    public CharacterCore Core { get; set; }
    public IDamageable Health { get; set; }
    public FactionsEnum OriginalFactionType { get; set; }

    public CharacterInfo(string name, FactionsEnum factionType, CharacterCore core, IDamageable health)
    {
        Name = name;
        FactionType = factionType;
        Core = core;
        Health = health;
        
        OriginalFactionType = FactionType;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    public void SetFactionType(FactionsEnum factionType)
    {
        FactionType = factionType;
    }
    
    public void ControlledByPlayer(bool isPlayerControlled)
    {
        if (isPlayerControlled)
        {
            OriginalFactionType = FactionType;
            SetFactionType(FactionsEnum.Player);
            return;
        }
        SetFactionType(OriginalFactionType);
    }
}
