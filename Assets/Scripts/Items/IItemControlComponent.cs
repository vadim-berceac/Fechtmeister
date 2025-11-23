using UnityEngine;

public interface IItemControlComponent
{
    public Collider Owner { get; set; }
    public IEquppiedItemData EquppiedItemData { get; set; }
    public bool ActionCompleted { get; set; }
    public void Use();
    public void ResetAction();
}

public abstract class ItemControlComponent<T> : IItemControlComponent where T : IEquppiedItemData
{
    public Collider Owner { get; set; }
    public IEquppiedItemData EquppiedItemData { get; set; }
    public bool ActionCompleted { get; set; }
    protected T TypedItemData => (T)EquppiedItemData;
    
    protected ItemControlComponent(Collider owner, T equppiedItemData)
    {
        Owner = owner;
        EquppiedItemData = equppiedItemData;
    }
    
    public abstract void Use();
    public abstract void ResetAction();
}
