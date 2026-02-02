using System;

public class ArmorSystem : IItemInstancesContainer
{
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IEquppiedItemData> OnItemEquipped { get; set; }
    public Action<IEquppiedItemData> OnItemUnEquipped { get; set; }
    public StateTimer StateTimer { get; set; }
    
    private readonly CharacterCore _characterCore;

    public ArmorSystem(int instancesCount, CharacterCore characterCore)
    {
        InstancesCount = instancesCount;
        _characterCore = characterCore;
        Instances = new IItemInstance[instancesCount];
    }
}
