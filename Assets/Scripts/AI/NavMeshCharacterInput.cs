using System;
using UnityEngine;
using UnityEngine.AI;

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

    private INavMeshState _currentState;
    private NavMeshStateData _stateData;

    private void Awake()
    {
        _stateData = new NavMeshStateData
        {
            NavMeshPath = new NavMeshPath(),
            Transform = transform,
            Settings = NavMeshSettings.Default
        };
        
        FindActions();
        Subscribe();
        
        ChangeState(new IdleBehaviorState());
        
        if (_stateData.Settings.AutoEnableOnStart)
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

    private void OnCharacterSelected(CharacterCore character)
    {
        if (character == null)
        {
            return;
        }
        SetTarget(character.CashedTransform);
    }

    public override void OnManagedUpdate()
    {
        if (!_stateData.IsEnabled)
        {
            return;
        }

        _currentState?.Update(ref _stateData, this);
    }

    public void FindActions()
    {
        if (_stateData.NavMeshPath == null)
        {
            _stateData.NavMeshPath = new NavMeshPath();
        }
        
        var triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices.Length == 0)
        {
            Debug.LogWarning("[NavMeshInput] NavMesh не найден в сцене! Убедитесь, что NavMesh запечён (baked).");
        }
    }

    public void Enable()
    {
        _stateData.IsEnabled = true;
       
        if (_stateData.TargetTransform != null)
        {
            ChangeState(new FollowTargetState());
        }
    }

    public void Disable()
    {
        _stateData.IsEnabled = false;
        OnMove?.Invoke(Vector2.zero);
        ChangeState(new IdleBehaviorState());
    }

    public void Subscribe()
    {
        CharacterSelector.OnCharacterSelected += OnCharacterSelected;
    }

    public void Unsubscribe()
    {
        CharacterSelector.OnCharacterSelected -= OnCharacterSelected;
        
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
        
        if (!_stateData.IsEnabled)
        {
            return;
        }
        
        if (target != null)
        {
            ChangeState(new FollowTargetState());
        }
        else
        {
            ChangeState(new IdleBehaviorState());
        }
    }

    public void StopFollowing()
    {
        OnMove?.Invoke(Vector2.zero);
        ChangeState(new IdleBehaviorState());
    }

    public void ClearTarget()
    {
        _stateData.TargetTransform = null;
        StopFollowing();
    }

    private void ChangeState(INavMeshState newState)
    {
        _currentState?.Exit(ref _stateData, this);
        _currentState = newState;
        _currentState?.Enter(ref _stateData, this);
    }

    public void InvokeMove(Vector2 moveInput) => OnMove?.Invoke(moveInput);
    
    public void SetRunState(bool shouldRun)
    {
        if (_stateData.IsRunning == shouldRun)
        {
            return; 
        }
        
        _stateData.IsRunning = shouldRun;
        OnRun?.Invoke(); 
    }
}