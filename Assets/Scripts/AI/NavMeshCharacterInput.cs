using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class NavMeshCharacterInput : ManagedUpdatableObject, ICharacterInputSet
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
    
    private float _chaseDuration;

    private IDamageable _health;
    private IDamageable _targetHealth;
    
    [SerializeField] private NavMeshSettings _settings = NavMeshSettings.Default;
    
    private NavMeshStateData _stateData;
    private NavMeshStateMachine _stateMachine;

    private void Awake()
    {
        TryGetComponent(out _health);
        InitializeStateData();
        ValidateNavMesh();
        Subscribe();
        
        _stateMachine = new NavMeshStateMachine(this);
        _stateMachine.ChangeState(NavMeshStateType.Idle, ref _stateData);
        
        if (_settings.AutoEnableOnStart)
        {
            Enable();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Unsubscribe();
        Disable();
    }

    public override void OnManagedUpdate()
    {
        if (!_stateData.IsEnabled) return;
        
        _stateMachine.Update(ref _stateData);
        
        CheckTargetHealth();
        UpdateChaseLogic();
    }
    
    public void FindActions(){}

    private void InitializeStateData()
    {
        _stateData = new NavMeshStateData
        {
            NavMeshPath = new NavMeshPath(),
            Transform = transform,
            Settings = _settings
        };
    }

    private void ValidateNavMesh()
    {
        var triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices.Length == 0)
        {
            Debug.LogWarning("[NavMeshInput] NavMesh not found in scene! Ensure NavMesh is baked.");
        }
    }

    public void Enable()
    {
        _stateData.IsEnabled = true;
        
        if (_stateData.TargetTransform != null)
        {
            _stateMachine.ChangeState(NavMeshStateType.Follow, ref _stateData);
        }
    }

    public void Disable()
    {
        _stateData.IsEnabled = false;
        OnMove?.Invoke(Vector2.zero);
        _stateMachine.ChangeState(NavMeshStateType.Idle, ref _stateData);
    }

    public void Subscribe()
    {
        if (_health != null)
        {
            _health.OnDamageAttempt += GetTargetDamageable;
        }
    }

    public void Unsubscribe()
    {
        if (_health != null)
        {
            _health.OnDamageAttempt -= GetTargetDamageable;
        }
        ClearAllEvents();
    }

    private void GetTargetDamageable(Transform target)
    {
        var damageable = target.GetComponent<IDamageable>();

        if (damageable != null)
        {
            _targetHealth = damageable;
            SetTarget(target);
        }
    }
    
    private void UpdateChaseLogic()
    {
        if (_stateData.TargetTransform == null) 
        {
            _chaseDuration = 0f;
            return;
        }
    
        var distance = (transform.position - _stateData.TargetTransform.position).magnitude;
       
        if (distance > _settings.MaxChaseDistance)
        {
            _chaseDuration += Time.deltaTime;
        
            if (_chaseDuration >= _settings.LoseInterestTime)
            {
                Debug.Log("[AI] Lost interest in target");
                ClearTarget();
            }
        }
        else
        {
            _chaseDuration = 0f;
        }
    }

    private void CheckTargetHealth()
    {
        if (_targetHealth == null)
        {
            return;
        }

        if (_targetHealth.IsDestroyed)
        {
            ClearTarget();
        }
    }

    private void ClearAllEvents()
    {
        OnAttack = null;
        OnAimBlock = null;
        OnInteract = null;
        OnJump = null;
        OnSneak = null;
        OnRun = null;
        OnDrawWeapon = null;
        OnHoldTarget = null;
        OnOpenInventory = null;
        OnWeaponSelect = null;
        OnMove = null;
        OnLook = null;
    }

    private void SetTarget(Transform target)
    {
        _stateData.TargetTransform = target;
        
        if (!_stateData.IsEnabled) return;
        
        var newState = target != null ? NavMeshStateType.Follow : NavMeshStateType.Idle;
        _stateMachine.ChangeState(newState, ref _stateData);
    }

    public void ClearTarget()
    {
        SetTarget(null);
    }

    // Public API для инвокации событий
    public void InvokeMove(Vector2 moveInput) => OnMove?.Invoke(moveInput);
    public void InvokeLook(Vector2 lookInput) => OnLook?.Invoke(lookInput);
    public void InvokeAttack() => OnAttack?.Invoke();
    public void InvokeDrawWeapon() => OnDrawWeapon?.Invoke();
    
    public void SetRunState(bool shouldRun)
    {
        if (_stateData.IsRunning == shouldRun) return;
        
        _stateData.IsRunning = shouldRun;
        OnRun?.Invoke();
    }
}