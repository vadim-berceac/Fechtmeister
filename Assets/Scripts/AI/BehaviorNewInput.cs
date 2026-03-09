using System;
using Unity.Behavior;
using UnityEngine;
using Zenject;
using Action = System.Action;

public class BehaviorNewInput : ManagedUpdatableObject, ICharacterInputSet
{
    [field: Header("Vision Settings")]
    [field: SerializeField] public float VisionRange { get; set; }
    [field: SerializeField] public float VisionAngle { get; set; }
    public BehaviorGraphAgent Agent { get; set; }
    public CharacterInfoComponent CharacterInfo { get; set; }
    public HealthComponent Health { get; set; }
    public CharacterCore Core { get; set; }
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
    public bool IsAimBlockActive { get; private set; }
    private VisionSystem _visionSystem;

    [Inject]
    private void Construct(VisionSystem visionSystem, HealthComponent healthComponent, CharacterCore characterCore,
        CharacterInfoComponent characterInfo, BehaviorGraphAgent agent)
    {
        _visionSystem = visionSystem;
        Health = healthComponent;
        Core = characterCore;
        CharacterInfo = characterInfo;
        Agent = agent;
    }
    
    public void SimulateMove(Vector2 direction) => OnMove?.Invoke(direction);
    public void SimulateLook(Vector2 direction) => OnLook?.Invoke(direction);
    public void SimulateAttack() => OnAttack?.Invoke();
    public void SimulateJump() => OnJump?.Invoke();
    public void SimulateBlock()
    {
        IsAimBlockActive = !IsAimBlockActive;
        OnAimBlock?.Invoke();
    }
    public void SimulateInteract() => OnInteract?.Invoke();
    public void SimulateSneak() => OnSneak?.Invoke();
    public void SwitchRunMode() => OnRun?.Invoke();
    public void SimulateDrawWeapon() => OnDrawWeapon?.Invoke();
    public void SimulateHoldTarget() => OnHoldTarget?.Invoke();
    public void SimulateOpenInventory() => OnOpenInventory?.Invoke();
    public void SimulateWeaponSelect(int weaponIndex) => OnWeaponSelect?.Invoke(weaponIndex);

    public override void OnManagedUpdate()
    {
        var enemy = _visionSystem.GetClosestHostileCharacter(CharacterInfo.CharacterInfo,
            VisionRange, VisionAngle);
        if (enemy != null)
        {
            SetHostileTarget(enemy.Health);
        }
    }
    
    public void FindActions(){}
    
    public void Enable()
    {
        IsEnabled = true;
    }

    public void Start()
    {
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
        Health.OnHitReaction += OnHitReactionUpdate;
        Core.Inventory.WeaponSystem.OnWeaponInHandsSelected += OnWeaponUpdate;
    }

    public void Unsubscribe()
    {
        _isSubscribed = false;
        Health.OnDestroyed -= OnDeath;
        Health.OnDamageAttempt -= OnPossibleDamage;
        Health.OnHitReaction -= OnHitReactionUpdate;
        Core.Inventory.WeaponSystem.OnWeaponInHandsSelected -= OnWeaponUpdate;
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
        source.TryGetComponent<HealthComponent>(out var targetHealth);
        SetHostileTarget(targetHealth);
    }

    private void SetHostileTarget(HealthComponent healthComponent)
    {
        Agent.BlackboardReference.SetVariableValue("CurrentTarget", healthComponent);
        Agent.BlackboardReference.SetVariableValue("IsInCombat", true);
    }

    private void OnWeaponUpdate(WeaponData data)
    {
        Agent.BlackboardReference.SetVariableValue("AttackRange", data.WeaponParams.PreferredDistance);
    }

    private void OnHitReactionUpdate(bool value)
    {
        Agent.BlackboardReference.SetVariableValue("OnHitReaction", value);
    }

    protected override void OnDisable()
    {
        CentralizedUpdateSystem.Instance.Unregister(this);
        Unsubscribe();
    }
}