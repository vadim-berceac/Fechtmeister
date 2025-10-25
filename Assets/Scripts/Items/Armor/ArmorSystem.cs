using System;

public class ArmorSystem : IItemInstancesContainer
{
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IItemData> OnItemUnEquipped { get; set; }
    public StateTimer StateTimer { get; set; }
    
    private readonly CharacterCore _characterCore;

    public ArmorSystem(int instancesCount, CharacterCore characterCore)
    {
        InstancesCount = instancesCount;
        _characterCore = characterCore;
        CharacterBonesContainer = _characterCore.BonesContainer;
        Instances = new IItemInstance[instancesCount];
    }
}
