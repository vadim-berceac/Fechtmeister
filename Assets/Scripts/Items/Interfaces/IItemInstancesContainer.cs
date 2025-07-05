using System;

public interface IItemInstancesContainer
{
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IItemData> OnItemUnEquipped { get; set; }
    
    public void Equip(IItemData item);
    public int GetEmptyInstance();
    public void DestroyInstance(int index);
}
