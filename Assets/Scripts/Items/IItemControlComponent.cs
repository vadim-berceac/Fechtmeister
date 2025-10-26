using UnityEngine;

public interface IItemControlComponent
{
    public Collider Owner { get; set; }
    public IItemData ItemData { get; set; }
    public bool ActionCompleted { get; set; }
    public void Use();
    public void ResetAction();
}

public abstract class ItemControlComponent<T> : IItemControlComponent where T : IItemData
{
    public Collider Owner { get; set; }
    public IItemData ItemData { get; set; }
    public bool ActionCompleted { get; set; }
    protected T TypedItemData => (T)ItemData;
    
    protected ItemControlComponent(Collider owner, T itemData)
    {
        Owner = owner;
        ItemData = itemData;
    }
    
    public abstract void Use();
    public abstract void ResetAction();
}
