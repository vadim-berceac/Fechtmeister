using System;
using UnityEngine;

public interface ICharacterInputSet : IInputSet
{
    public event Action OnAttack;
    public event Action OnAimBlock;
    public event Action OnInteract;
    public event Action OnJump;
    public event Action OnSneak;
    public event Action OnRun;
    public event Action OnDrawWeapon;
    public event Action OnHoldTarget;
    public event Action OnOpenInventory;
    public event Action<int> OnWeaponSelect;
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public int SelectedWeapon { get; set; }
}

public static class ICharacterInputSetExtensions
{
    public static void CheckSelectedWeapons(this ICharacterInputSet set, int index, Action<int> weaponAction, Action onTrueAction)
    {
        weaponAction?.Invoke(index);
        if (set.SelectedWeapon == index)
        {
            onTrueAction?.Invoke();
        }
        set.SelectedWeapon = index;
    }
}