using System.Collections.Generic;
using UnityEngine;

public class CentralizedUpdateSystem : MonoBehaviour
{
    public static CentralizedUpdateSystem Instance { get; private set; }

    [Header("LOD Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float visibleCloseDistance = 20f;
    [SerializeField] private float visibleMediumDistance = 50f;
    [SerializeField] private float invisibleCloseDistance = 30f;
    [SerializeField] private float invisibleMediumDistance = 80f;

    [Header("Performance")]
    [SerializeField] private int maxUpdatesPerFrame = 100;
    [SerializeField] private bool enableDebug = false;

    private List<IUpdatable> allObjects = new List<IUpdatable>();
    private Dictionary<IUpdatable, UpdateLOD> objectLODs = new Dictionary<IUpdatable, UpdateLOD>();
    
    // Счётчики для Medium и Low LOD
    private int frameCounter = 0;
    private int mediumLODOffset = 0;
    private int lowLODOffset = 0;

    // Кэш для оптимизации
    private Plane[] cameraFrustumPlanes;
    private int lodUpdateInterval = 10; // Обновлять LOD каждые N кадров
    private int lodUpdateCounter = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        frameCounter++;
        lodUpdateCounter++;

        // Периодически обновляем LOD уровни
        if (lodUpdateCounter >= lodUpdateInterval)
        {
            UpdateLODLevels();
            lodUpdateCounter = 0;
        }

        // Обновляем объекты согласно их LOD
        int updatesThisFrame = 0;
        foreach (var obj in allObjects)
        {
            if (updatesThisFrame >= maxUpdatesPerFrame)
                break;

            if (ShouldUpdate(obj, objectLODs[obj]))
            {
                obj.OnManagedUpdate();
                updatesThisFrame++;
            }
        }
    }

    private void FixedUpdate()
    {
        int updatesThisFrame = 0;
        foreach (var obj in allObjects)
        {
            if (updatesThisFrame >= maxUpdatesPerFrame)
                break;

            if (ShouldUpdate(obj, objectLODs[obj]))
            {
                obj.OnManagedFixedUpdate();
                updatesThisFrame++;
            }
        }
    }

    private void LateUpdate()
    {
        int updatesThisFrame = 0;
        foreach (var obj in allObjects)
        {
            if (updatesThisFrame >= maxUpdatesPerFrame)
                break;

            if (ShouldUpdate(obj, objectLODs[obj]))
            {
                obj.OnManagedLateUpdate();
                updatesThisFrame++;
            }
        }
    }

    /// <summary>
    /// Регистрирует объект в системе обновления
    /// </summary>
    public void Register(IUpdatable obj)
    {
        if (!allObjects.Contains(obj))
        {
            allObjects.Add(obj);
            objectLODs[obj] = UpdateLOD.Full;
        }
    }

    /// <summary>
    /// Удаляет объект из системы обновления
    /// </summary>
    public void Unregister(IUpdatable obj)
    {
        allObjects.Remove(obj);
        objectLODs.Remove(obj);
    }

    /// <summary>
    /// Проверяет, должен ли объект обновиться в этом кадре
    /// </summary>
    private bool ShouldUpdate(IUpdatable obj, UpdateLOD lod)
    {
        switch (lod)
        {
            case UpdateLOD.Full:
                return true;
            
            case UpdateLOD.Medium:
                // Обновляем 2 из 3 кадров (пропускаем каждый третий)
                return (frameCounter + obj.GetHashCode() + mediumLODOffset) % 3 != 0;
            
            case UpdateLOD.Low:
                // Обновляем 1 из 3 кадров
                return (frameCounter + obj.GetHashCode() + lowLODOffset) % 3 == 0;
            
            case UpdateLOD.Paused:
                return false;
            
            default:
                return true;
        }
    }

    /// <summary>
    /// Обновляет LOD уровни для всех объектов
    /// </summary>
    private void UpdateLODLevels()
    {
        if (mainCamera == null)
            return;

        cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        Vector3 cameraPos = mainCamera.transform.position;

        foreach (var obj in allObjects)
        {
            if (obj.Transform == null)
                continue;

            Vector3 objPos = obj.Transform.position;
            float distance = Vector3.Distance(cameraPos, objPos);
            bool isVisible = IsVisibleByCamera(obj.Transform);

            UpdateLOD newLOD = CalculateLOD(distance, isVisible);
            objectLODs[obj] = newLOD;

            if (enableDebug)
                Debug.Log($"{obj.Transform.name}: Distance={distance:F1}, Visible={isVisible}, LOD={newLOD}");
        }
    }

    /// <summary>
    /// Проверяет, виден ли объект камерой
    /// </summary>
    private bool IsVisibleByCamera(Transform transform)
    {
        // Простая проверка по bounds (можно заменить на более точную)
        Bounds bounds = new Bounds(transform.position, Vector3.one * 2f);
        return GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, bounds);
    }

    /// <summary>
    /// Вычисляет LOD уровень на основе дистанции и видимости
    /// </summary>
    private UpdateLOD CalculateLOD(float distance, bool isVisible)
    {
        if (isVisible)
        {
            // В пределах видимости камеры
            if (distance < visibleCloseDistance)
                return UpdateLOD.Full;
            else if (distance < visibleMediumDistance)
                return UpdateLOD.Medium;
            else
                return UpdateLOD.Low;
        }
        else
        {
            // Вне видимости камеры
            if (distance < invisibleCloseDistance)
                return UpdateLOD.Full;
            else if (distance < invisibleMediumDistance)
                return UpdateLOD.Low;
            else
                return UpdateLOD.Paused;
        }
    }

    /// <summary>
    /// Принудительно устанавливает LOD для конкретного объекта
    /// </summary>
    public void SetLOD(IUpdatable obj, UpdateLOD lod)
    {
        if (objectLODs.ContainsKey(obj))
            objectLODs[obj] = lod;
    }

    /// <summary>
    /// Возвращает текущий LOD объекта
    /// </summary>
    public UpdateLOD GetLOD(IUpdatable obj)
    {
        return objectLODs.ContainsKey(obj) ? objectLODs[obj] : UpdateLOD.Paused;
    }

    /// <summary>
    /// Возвращает количество зарегистрированных объектов
    /// </summary>
    public int GetRegisteredCount()
    {
        return allObjects.Count;
    }

    private void OnDrawGizmosSelected()
    {
        if (mainCamera == null)
            return;

        Vector3 cameraPos = mainCamera.transform.position;

        // Видимые зоны
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(cameraPos, visibleCloseDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cameraPos, visibleMediumDistance);

        // Невидимые зоны
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(cameraPos, invisibleCloseDistance);
        
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(cameraPos, invisibleMediumDistance);
    }
}