using UnityEngine;

public interface IGameWindow
{
    public bool IsActive();
    public void Open();
    public void Close();
    public void Register();
    public void BLockPlayerInput(bool value);
}
