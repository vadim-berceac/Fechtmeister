
public class CharacterInfo
{
    public string Name { get; set; }
    public Faction Faction { get; set; }
    public CharacterCore Core { get; set; }
    public HealthComponent Health { get; set; }
    public Faction OriginalFaction { get; set; }

    public CharacterInfo(string name, FactionsEnum factionType, CharacterCore core, HealthComponent health)
    {
        Name = name;
        Faction = Factions.GetFaction(factionType);
        Core = core;
        Health = health;
       
        OriginalFaction = Faction;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    public void SetFactionType(FactionsEnum factionType)
    {
        Faction = Factions.GetFaction(factionType);
    }
    
    public void ControlledByPlayer(bool isPlayerControlled)
    {
        if (isPlayerControlled)
        {
            OriginalFaction = Faction;
            SetFactionType(FactionsEnum.Player);
            return;
        }
        SetFactionType(OriginalFaction.FactionType);
    }
}
