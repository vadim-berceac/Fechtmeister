using System;
using Unity.Behavior;
using UnityEngine;
using Action = System.Action;

public class BehaviorNewInput : ManagedUpdatableObject, ICharacterInputSet
{
    [field: SerializeField] public HealthComponent Health { get; set; }
    [field: SerializeField] public BehaviorGraphAgent Agent { get; set; }
    
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
    public bool IsInCombatMode { get; private set; }
    private bool _isSubscribed;
    
    public void SimulateMove(Vector2 direction) => OnMove?.Invoke(direction);
    public void SimulateLook(Vector2 direction) => OnLook?.Invoke(direction);
    public void SimulateAttack() => OnAttack?.Invoke();
    public void SimulateJump() => OnJump?.Invoke();
    public void SimulateBlock() => OnAimBlock?.Invoke();
    public void SimulateInteract() => OnInteract?.Invoke();
    public void SimulateSneak() => OnSneak?.Invoke();
    public void SimulateRun() => OnRun?.Invoke();
    public void SimulateDrawWeapon() => OnDrawWeapon?.Invoke();
    public void SimulateHoldTarget() => OnHoldTarget?.Invoke();
    public void SimulateOpenInventory() => OnOpenInventory?.Invoke();
    public void SimulateWeaponSelect(int weaponIndex) => OnWeaponSelect?.Invoke(weaponIndex);
    
    public override void OnManagedUpdate(){}
    
    public void FindActions(){}
    
    public void Enable()
    {
        IsEnabled = true;
        Subscribe();
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void Subscribe()
    {
        if (_isSubscribed) return;
        
        _isSubscribed = true;
        Health.OnDestroyed += OnDeath;
        Health.OnDamageAttempt += OnPossibleDamage;
    }

    public void Unsubscribe()
    {
        _isSubscribed = false;
        Health.OnDestroyed -= OnDeath;
        Health.OnDamageAttempt -= OnPossibleDamage;
    }

    private void OnDeath(bool isDead)
    {
        if (isDead)
        {
            Disable();
            return;
        }
        Enable();
    }

    private void OnPossibleDamage(Transform source)
    {
        IsInCombatMode = true;
        
        Agent.BlackboardReference.SetVariableValue("CurrentTarget", source.gameObject);
        Agent.BlackboardReference.SetVariableValue("IsInCombat", true);
    }

    protected override void OnDisable()
    {
        Unsubscribe();
    }
}