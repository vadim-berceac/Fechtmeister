using System;
using UnityEngine;

public interface ICharacterInputSet : IInputSet
{
    public event Action OnAttack;
    public event Action OnInteract;
    public event Action OnJump;
    public event Action OnSneak;
    public event Action OnSprint;
    public event Action OmDrawWeapon;
    public event Action OnHoldTarget;
    public event Action OnOpenInventory;
    public event Action OnWeaponSelect0;
    public event Action OnWeaponSelect1;
    public event Action OnWeaponSelect2;
    public Vector2 Move { get; }
    public Vector2 Look { get; }
}
