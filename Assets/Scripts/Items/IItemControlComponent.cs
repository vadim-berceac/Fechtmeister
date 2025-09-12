using UnityEngine;

public interface IItemControlComponent
{
    public bool IsAllowed  { get; set; }
    public Collider Owner { get; set; }
    public void AllowToUse (bool isAllowed);
    public void SetOwner(Collider owner);
}
