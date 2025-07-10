using System;
using UnityEngine;

public interface ICharacterInputSet : IInputSet
{
    public event Action OnAttack;
    public event Action OnInteract;
    public event Action OnJump;
    public event Action OnSneak;
    public event Action OnRun;
    public event Action OnDrawWeapon;
    public event Action OnHoldTarget;
    public event Action OnOpenInventory;
    public event Action OnWeaponSelect0;
    public event Action OnWeaponSelect1;
    public event Action OnWeaponSelect2;
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
}
