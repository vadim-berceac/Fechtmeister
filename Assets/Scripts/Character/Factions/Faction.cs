
public class Faction
{
    private readonly string _name;
    private readonly FactionsEnum _factionType;
    private Faction[] Hostiles { get; set; }
    
    public string Name => _name;
    public FactionsEnum FactionType => _factionType;

    private Faction(string name, FactionsEnum factionType)
    {
        _name = name;
        _factionType = factionType;
        Hostiles = System.Array.Empty<Faction>();
    }

    internal void SetHostiles(Faction[] hostiles)
    {
        Hostiles = hostiles;
    }

    public bool EqualsTo(Faction faction)
    {
        return _factionType == faction.FactionType;
    }

    public bool IsHostileTo(Faction faction)
    {
        if (faction == null) return false;
        
        foreach (var hostile in Hostiles)
        {
            if (hostile.EqualsTo(faction))
                return true;
        }
        return false;
    }

    internal static Faction Create(string name, FactionsEnum factionType)
    {
        return new Faction(name, factionType);
    }
}

public static class Factions
{
    private static readonly Faction None;
    private static readonly Faction Player;
    private static readonly Faction Bandit;
    private static readonly Faction Undead;

    static Factions()
    {
        None = Faction.Create("None", FactionsEnum.None);
        Player = Faction.Create("Player", FactionsEnum.Player);
        Bandit = Faction.Create("Bandit", FactionsEnum.Bandit);
        Undead = Faction.Create("Undead", FactionsEnum.Undead);

        Player.SetHostiles(new[] { Bandit, Undead });
        Bandit.SetHostiles(new[] { Player, Undead });
        Undead.SetHostiles(new[] { Player, Bandit });
    }
    
    public static Faction GetFaction(FactionsEnum factionType)
    {
        return factionType switch
        {
            FactionsEnum.None => None,
            FactionsEnum.Player => Player,
            FactionsEnum.Bandit => Bandit,
            FactionsEnum.Undead => Undead,
            _ => None
        };
    }
}