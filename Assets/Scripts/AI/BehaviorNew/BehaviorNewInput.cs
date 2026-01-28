using System;
using UnityEngine;

public class BehaviorNewInput : ManagedUpdatableObject, ICharacterInputSet
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
    public bool IsEnabled { get; set; }

    public int SelectedWeapon { get; set; }
    
    public override void OnManagedUpdate()
    {
       
    }
    
    public void FindActions(){}
    
    public void Enable()
    {
       
    }

    public void Disable()
    {
      
    }

    public void Subscribe()
    {
       
    }

    public void Unsubscribe()
    {
       
    }
}