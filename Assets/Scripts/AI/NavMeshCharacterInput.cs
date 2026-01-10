using System;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshCharacterInput : ManagedUpdatableObject, ICharacterInputSet
{
    [Header("Navigation Settings")]
    [SerializeField] private float pathUpdateInterval = 0.5f;
    [SerializeField] private float waypointReachDistance = 1.5f; 
    [SerializeField] private float finalDestinationDistance = 2f; 
    [SerializeField] private float pathRecalculationThreshold = 2f; 
    [SerializeField] private float navMeshSampleDistance = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float idleRotationSpeed = 3f;
    [SerializeField] private float stuckDetectionTime = 2f; 
    [SerializeField] private float minProgressDistance = 0.5f; 
    
    [Header("Debug")]
    [SerializeField] private bool autoEnableOnStart = true;
    
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

    private Transform _targetTransform;
    
    private NavMeshPath _navMeshPath;
    private int _currentWaypointIndex;
    private float _nextPathUpdateTime;
    private Vector3 _lastTargetPosition;
    private Vector3 _lastCharacterPosition;
    private float _lastProgressCheckTime;
    private float _lastDistanceToTarget;
    private bool _isFollowing;
    private bool _isEnabled;
    private bool _hasReachedDestination;

    private void Awake()
    {
        _navMeshPath = new NavMeshPath();
        
        FindActions();
        Subscribe();
        
        if (!autoEnableOnStart)
        {
            return;
        }
        
        Enable();
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
        if (!_isEnabled || _targetTransform == null)
        {
            return;
        }

        if (_hasReachedDestination)
        {
            RotateTowardsTarget(_targetTransform.position, idleRotationSpeed);
        }

        if (_isFollowing)
        {
            UpdatePathFollowing();
        }
    }

    public void FindActions()
    {
        if (_navMeshPath == null)
        {
            _navMeshPath = new NavMeshPath();
        }
        
        var triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices.Length == 0)
        {
            Debug.LogWarning("[NavMeshInput] NavMesh не найден в сцене! Убедитесь, что NavMesh запечён (baked).");
        }
    }

    public void Enable()
    {
        _isEnabled = true;
       
        if (_targetTransform == null)
        {
           return;
        }
        _isFollowing = true;
        _hasReachedDestination = false;
        _lastCharacterPosition = transform.position;
        _lastProgressCheckTime = Time.time;
        _lastDistanceToTarget = float.MaxValue;
        CalculatePath();
    }

    public void Disable()
    {
        _isEnabled = false;
        OnMove?.Invoke(Vector2.zero);
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
        _targetTransform = target;
        _isFollowing = true;
        _hasReachedDestination = false;
        _lastCharacterPosition = transform.position;
        _lastProgressCheckTime = Time.time;
        _lastDistanceToTarget = float.MaxValue;
        
        if (!_isEnabled)
        {
            return;
        }
        
        CalculatePath();
    }

    public void StopFollowing()
    {
        _isFollowing = false;
        _hasReachedDestination = true;
        OnMove?.Invoke(Vector2.zero);
    }

    public void ClearTarget()
    {
        _targetTransform = null;
        StopFollowing();
    }

    private void UpdatePathFollowing()
    {
        if (!_isFollowing || _targetTransform == null)
        {
            return;
        }

        var distanceToTarget = Vector3.Distance(transform.position, _targetTransform.position);

        if (distanceToTarget <= finalDestinationDistance * 2f && 
            distanceToTarget > _lastDistanceToTarget && 
            _lastDistanceToTarget <= finalDestinationDistance * 1.2f)
        {
            _hasReachedDestination = true;
            OnMove?.Invoke(Vector2.zero);
            return;
        }
        
        _lastDistanceToTarget = distanceToTarget;

        if (_hasReachedDestination && distanceToTarget > finalDestinationDistance * 1.5f)
        {
            _hasReachedDestination = false;
            _lastCharacterPosition = transform.position;
            _lastProgressCheckTime = Time.time;
            _lastDistanceToTarget = float.MaxValue;
            CalculatePath();
        }

        if (distanceToTarget <= finalDestinationDistance)
        {
            if (!_hasReachedDestination)
            {
                _hasReachedDestination = true;
                OnMove?.Invoke(Vector2.zero);
            }
            return;
        }

        if (Time.time - _lastProgressCheckTime > stuckDetectionTime)
        {
            var progressDistance = Vector3.Distance(transform.position, _lastCharacterPosition);
          
            if (progressDistance < minProgressDistance && distanceToTarget > finalDestinationDistance)
            {
                _hasReachedDestination = true;
                OnMove?.Invoke(Vector2.zero);
                return;
            }
            
            _lastCharacterPosition = transform.position;
            _lastProgressCheckTime = Time.time;
        }

        var shouldRecalculate = Time.time >= _nextPathUpdateTime;

        if (_targetTransform != null)
        {
            var targetMoved = Vector3.Distance(_targetTransform.position, _lastTargetPosition);
            if (targetMoved > pathRecalculationThreshold)
            {
                shouldRecalculate = true;
            }
        }

        if (shouldRecalculate)
        {
            CalculatePath();
            _nextPathUpdateTime = Time.time + pathUpdateInterval;
        }

        if (_navMeshPath == null || 
            _navMeshPath.status != NavMeshPathStatus.PathComplete || 
            _navMeshPath.corners.Length == 0)
        {
            return;
        }

        FollowPath();
    }
    
    private void CalculatePath()
    {
        if (_targetTransform == null)
        {
            return;
        }

        var startPos = transform.position;
        var targetPos = _targetTransform.position;

        var startFound = NavMesh.SamplePosition(startPos, out var startHit, navMeshSampleDistance, NavMesh.AllAreas);
        var targetFound = NavMesh.SamplePosition(targetPos, out var targetHit, navMeshSampleDistance, NavMesh.AllAreas);

        if (!startFound || !targetFound)
        {
            return;
        }

        var pathCalculated = NavMesh.CalculatePath(startHit.position, targetHit.position, NavMesh.AllAreas, _navMeshPath);
        
        if (!pathCalculated || _navMeshPath.status != NavMeshPathStatus.PathComplete)
        {
            return;
        }

        _currentWaypointIndex = 0;
        _lastTargetPosition = targetPos;
    }

    private void FollowPath()
    {
        if (_targetTransform != null)
        {
            var currentDistanceToTarget = Vector3.Distance(transform.position, _targetTransform.position);
           
            if (currentDistanceToTarget <= finalDestinationDistance)
            {
                if (!_hasReachedDestination)
                {
                    _hasReachedDestination = true;
                    OnMove?.Invoke(Vector2.zero);
                }
                return;
            }
        }
        
        if (_currentWaypointIndex >= _navMeshPath.corners.Length)
        {
            OnMove?.Invoke(Vector2.zero);
            return;
        }

        var targetWaypoint = _navMeshPath.corners[_currentWaypointIndex];
        var horizontalPosition = new Vector3(transform.position.x, 0, transform.position.z);
        var horizontalWaypoint = new Vector3(targetWaypoint.x, 0, targetWaypoint.z);
        var distanceToWaypoint = Vector3.Distance(horizontalPosition, horizontalWaypoint);

        if (distanceToWaypoint < waypointReachDistance)
        {
            _currentWaypointIndex++;

            if (_currentWaypointIndex >= _navMeshPath.corners.Length)
            {
                if (_targetTransform == null)
                {
                    OnMove?.Invoke(Vector2.zero);
                    return;
                }

                var finalDistance = Vector3.Distance(transform.position, _targetTransform.position);
                if (finalDistance <= finalDestinationDistance)
                {
                    _hasReachedDestination = true;
                    OnMove?.Invoke(Vector2.zero);
                    return;
                }

                CalculatePath();
                return;
            }

            targetWaypoint = _navMeshPath.corners[_currentWaypointIndex];
            
            if (_targetTransform != null)
            {
                var distCheck = Vector3.Distance(transform.position, _targetTransform.position);
                if (distCheck <= finalDestinationDistance)
                {
                    _hasReachedDestination = true;
                    OnMove?.Invoke(Vector2.zero);
                    return;
                }
            }
        }

        var directionWorld = targetWaypoint - transform.position;
        directionWorld.y = 0;

        if (directionWorld.magnitude <= 0.01f)
        {
            OnMove?.Invoke(Vector2.zero);
            return;
        }

        directionWorld.Normalize();
        
        RotateTowardsTarget(targetWaypoint, rotationSpeed);

        var localDirection = transform.InverseTransformDirection(directionWorld);
        var moveInput = new Vector2(localDirection.x, localDirection.z).normalized;

        OnMove?.Invoke(moveInput);
    }

    private void RotateTowardsTarget(Vector3 targetPosition, float speed)
    {
        var directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0; 
        
        if (directionToTarget.magnitude < 0.01f)
        {
            return; 
        }
        
        directionToTarget.Normalize();
        var targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
    }
}