using System;

public class ArmorSystem : IItemInstancesContainer
{
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IItemData> OnItemUnEquipped { get; set; }

    public ArmorSystem()
    {
        
    }
}
