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
    
    [SerializeField] private NavMeshSettings _settings = NavMeshSettings.Default;
    
    private NavMeshStateData _stateData;
    private NavMeshStateMachine _stateMachine;

    private void Awake()
    {
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

    private void OnCharacterSelected(CharacterCore character)
    {
        if (character == null) return;
        
        if (Random.Range(0, 2) > 0)
        {
            SetTarget(character.CashedTransform);
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
        CharacterSelector.OnCharacterSelected += OnCharacterSelected;
    }

    public void Unsubscribe()
    {
        CharacterSelector.OnCharacterSelected -= OnCharacterSelected;
        ClearAllEvents();
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

    public void SetTarget(Transform target)
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