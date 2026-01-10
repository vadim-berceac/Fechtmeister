using System;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshCharacterInput : MonoBehaviour, ICharacterInputSet
{
    [Header("Navigation Settings")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float pathUpdateInterval = 0.5f;
    [SerializeField] private float stoppingDistance = 0.5f;
    [SerializeField] private float pathRecalculationThreshold = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool autoEnableOnStart = true;

    // События интерфейса
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

    // Данные пути NavMesh
    private NavMeshPath navMeshPath;
    private int currentWaypointIndex;
    private float nextPathUpdateTime;
    private Vector3 lastTargetPosition;
    private bool isFollowing;
    private bool isEnabled;
    
    // Для отслеживания изменения цели в инспекторе
    private Transform previousTarget;

    private void Awake()
    {
        navMeshPath = new NavMeshPath();
        previousTarget = targetTransform;
        
        if (showDebugLogs)
            Debug.Log($"[NavMeshInput] Awake - Component initialized on {gameObject.name}");
    }

    private void Start()
    {
        // ВАЖНО: Вызываем FindActions для инициализации
        FindActions();
        Subscribe();
        
        if (autoEnableOnStart)
        {
            Enable();
            
            if (targetTransform != null)
            {
                SetTarget(targetTransform);
            }
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnValidate()
    {
        // Отслеживаем изменение цели в инспекторе в рантайме
        if (Application.isPlaying && targetTransform != previousTarget)
        {
            previousTarget = targetTransform;
            
            if (showDebugLogs)
                Debug.Log($"[NavMeshInput] Target changed in inspector to: {(targetTransform ? targetTransform.name : "null")}");
            
            if (targetTransform != null)
            {
                SetTarget(targetTransform);
            }
            else
            {
                StopFollowing();
            }
        }
    }

    private void OnCharacterSelected(CharacterCore character)
    {
        if (character == null)
        {
            return;
        }
        SetTarget(character.CashedTransform);
    }

    private void Update()
    {
        if (isEnabled && targetTransform != null)
        {
            UpdatePathFollowing();
        }
    }

    public void FindActions()
    {
        if (navMeshPath == null)
        {
            navMeshPath = new NavMeshPath();
        }
        
        // Проверяем наличие NavMesh в сцене
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices.Length == 0)
        {
            Debug.LogWarning("[NavMeshInput] NavMesh не найден в сцене! Убедитесь, что NavMesh запечён (baked).");
        }
        else if (showDebugLogs)
        {
            Debug.Log($"[NavMeshInput] NavMesh найден: {triangulation.vertices.Length} вершин");
        }
    }

    public void Enable()
    {
        isEnabled = true;
        
        if (showDebugLogs)
            Debug.Log($"[NavMeshInput] ENABLED on {gameObject.name}");
        
        // Если есть установленная цель, начинаем следование
        if (targetTransform != null)
        {
            isFollowing = true;
            CalculatePath();
        }
    }

    public void Disable()
    {
        isEnabled = false;
        
        if (showDebugLogs)
            Debug.Log($"[NavMeshInput] DISABLED on {gameObject.name}");
        
        // Останавливаем движение при отключении
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

    /// <summary>
    /// Устанавливает цель для следования
    /// </summary>
    public void SetTarget(Transform target)
    {
        targetTransform = target;
        previousTarget = target;
        isFollowing = true;
        
        if (showDebugLogs)
            Debug.Log($"[NavMeshInput] SetTarget: {(target ? target.name : "null")} | isEnabled: {isEnabled}");
        
        if (isEnabled)
        {
            CalculatePath();
        }
        else
        {
            Debug.LogWarning($"[NavMeshInput] Target set but component is DISABLED! Call Enable() first.");
        }
    }

    /// <summary>
    /// Останавливает следование за целью
    /// </summary>
    public void StopFollowing()
    {
        isFollowing = false;
        OnMove?.Invoke(Vector2.zero);
        
        if (showDebugLogs)
            Debug.Log($"[NavMeshInput] StopFollowing called");
    }

    /// <summary>
    /// Полностью очищает цель
    /// </summary>
    public void ClearTarget()
    {
        targetTransform = null;
        previousTarget = null;
        StopFollowing();
        
        if (showDebugLogs)
            Debug.Log($"[NavMeshInput] ClearTarget called");
    }

    private void UpdatePathFollowing()
    {
        if (!isFollowing) return;

        // Проверка необходимости пересчета пути
        bool shouldRecalculate = Time.time >= nextPathUpdateTime;
        
        if (targetTransform != null)
        {
            float distanceToLastTarget = Vector3.Distance(targetTransform.position, lastTargetPosition);
            if (distanceToLastTarget > pathRecalculationThreshold)
            {
                shouldRecalculate = true;
            }
        }

        if (shouldRecalculate)
        {
            CalculatePath();
            nextPathUpdateTime = Time.time + pathUpdateInterval;
        }

        // Следование по пути
        if (navMeshPath.status == NavMeshPathStatus.PathComplete && navMeshPath.corners.Length > 0)
        {
            FollowPath();
        }
        else if (navMeshPath.status != NavMeshPathStatus.PathComplete && showDebugLogs)
        {
            Debug.LogWarning($"[NavMeshInput] Path status: {navMeshPath.status}");
        }
    }

    private void CalculatePath()
    {
        if (targetTransform == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("[NavMeshInput] CalculatePath: targetTransform is null");
            return;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = targetTransform.position;

        // Находим ближайшую точку на NavMesh для начальной и конечной позиций
        NavMeshHit startHit, targetHit;
        bool startFound = NavMesh.SamplePosition(startPos, out startHit, 5f, NavMesh.AllAreas);
        bool targetFound = NavMesh.SamplePosition(targetPos, out targetHit, 5f, NavMesh.AllAreas);

        if (showDebugLogs)
        {
            Debug.Log($"[NavMeshInput] CalculatePath: Start found: {startFound} | Target found: {targetFound}");
            Debug.Log($"[NavMeshInput] Start pos: {startPos} -> NavMesh: {startHit.position}");
            Debug.Log($"[NavMeshInput] Target pos: {targetPos} -> NavMesh: {targetHit.position}");
        }

        if (startFound && targetFound)
        {
            bool pathCalculated = NavMesh.CalculatePath(startHit.position, targetHit.position, NavMesh.AllAreas, navMeshPath);
            currentWaypointIndex = 0;
            lastTargetPosition = targetPos;
            
            if (showDebugLogs)
            {
                Debug.Log($"[NavMeshInput] Path calculated: {pathCalculated} | Status: {navMeshPath.status} | Corners: {navMeshPath.corners.Length}");
                
                if (navMeshPath.corners.Length > 0)
                {
                    for (int i = 0; i < navMeshPath.corners.Length; i++)
                    {
                        Debug.Log($"[NavMeshInput] Corner {i}: {navMeshPath.corners[i]}");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"[NavMeshInput] Не удалось найти путь на NavMesh! StartFound: {startFound}, TargetFound: {targetFound}");
        }
    }

    private void FollowPath()
    {
        if (currentWaypointIndex >= navMeshPath.corners.Length)
        {
            OnMove?.Invoke(Vector2.zero);
            return;
        }

        Vector3 targetWaypoint = navMeshPath.corners[currentWaypointIndex];
        Vector3 direction = (targetWaypoint - transform.position);
        float distanceToWaypoint = direction.magnitude;

        // Достигли текущей точки пути
        if (distanceToWaypoint < stoppingDistance)
        {
            currentWaypointIndex++;
            
            if (showDebugLogs)
                Debug.Log($"[NavMeshInput] Reached waypoint {currentWaypointIndex - 1}, moving to next");
            
            // Достигли конечной точки
            if (currentWaypointIndex >= navMeshPath.corners.Length)
            {
                OnMove?.Invoke(Vector2.zero);
                
                if (showDebugLogs)
                    Debug.Log($"[NavMeshInput] Reached final destination!");
                    
                return;
            }
            
            targetWaypoint = navMeshPath.corners[currentWaypointIndex];
            direction = (targetWaypoint - transform.position);
        }

        // Нормализация направления (игнорируем Y для горизонтального движения)
        direction.y = 0;
        direction.Normalize();

        // Движение к точке
        if (direction.magnitude > 0.01f)
        {
            // Преобразуем 3D направление в 2D вектор движения
            Vector2 moveInput = new Vector2(direction.x, direction.z).normalized;
            
            if (showDebugLogs && Time.frameCount % 60 == 0) // Лог каждую секунду
            {
                Debug.Log($"[NavMeshInput] Sending moveInput: {moveInput} | Subscribers: {OnMove?.GetInvocationList().Length ?? 0}");
            }
            
            // ГЛАВНОЕ: вызываем событие движения для системы персонажа
            OnMove?.Invoke(moveInput);

            // Проверяем, подписан ли кто-то на событие OnMove
            if (OnMove == null)
            {
                if (showDebugLogs && Time.frameCount % 120 == 0)
                {
                    Debug.LogWarning("[NavMeshInput] OnMove event has NO subscribers! " +
                        "Your character movement system needs to subscribe to this event!");
                }
            }
        }
    }

    /// <summary>
    /// Рисует путь в Scene View для отладки
    /// </summary>
    private void OnDrawGizmos()
    {
        if (navMeshPath == null || navMeshPath.corners.Length == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
        {
            Gizmos.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1]);
            Gizmos.DrawSphere(navMeshPath.corners[i], 0.1f);
        }

        if (navMeshPath.corners.Length > 0)
        {
            Gizmos.DrawSphere(navMeshPath.corners[navMeshPath.corners.Length - 1], 0.1f);
        }

        // Отображение текущей целевой точки
        if (currentWaypointIndex < navMeshPath.corners.Length)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(navMeshPath.corners[currentWaypointIndex], 0.2f);
        }
        
        // Линия к цели
        if (targetTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetTransform.position);
        }
    }
}