using UnityEngine;

public interface IItemInstancesContainer
{
    public IItemInstance[] Instances { get; set; }
    
    public void Equip(IItemData item);
    public void UnEquip(IItemInstance itemInstance);
}
