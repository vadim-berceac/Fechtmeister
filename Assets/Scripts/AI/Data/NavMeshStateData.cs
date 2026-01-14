using UnityEngine;
using UnityEngine.AI;

public struct NavMeshStateData
{
    // Основные ссылки
    public Transform Transform;
    public Transform TargetTransform;
    public NavMeshPath NavMeshPath;
    public NavMeshSettings Settings;
    
    // Навигация
    public int CurrentWaypointIndex;
    public float NextPathUpdateTime;
    public Vector3 LastTargetPosition;
    
    // Отслеживание движения
    public Vector3 LastCharacterPosition;
    public float LastProgressCheckTime;
    
    // Состояние
    public bool IsEnabled;
    public bool IsStuck;
    public bool HasReachedDestination;
    public bool IsRunning;
    public bool HasWeaponDrawn;
    
    // Атака
    public float LastAttackTime;
}

[System.Serializable]
public struct NavMeshSettings
{
    [Header("General")]
    public bool AutoEnableOnStart;
    
    [Header("Pathfinding")]
    [Tooltip("Как часто пересчитывать путь (секунды)")]
    public float PathUpdateInterval;
    
    [Tooltip("Расстояние для считывания достижения waypoint")]
    public float WaypointReachDistance;
    
    [Tooltip("Финальная дистанция до цели для остановки")]
    public float FinalDestinationDistance;
    
    [Tooltip("Насколько должна переместиться цель для пересчёта пути")]
    public float PathRecalculationThreshold;
    
    [Tooltip("Расстояние поиска ближайшей точки на NavMesh")]
    public float NavMeshSampleDistance;
    
    [Header("Rotation")]
    [Tooltip("Скорость поворота при движении")]
    public float RotationSpeed;
    
    [Tooltip("Скорость поворота в idle")]
    public float IdleRotationSpeed;
    
    [Tooltip("Скорость поворота в атаке")]
    public float AttackRotationSpeed;
    
    [Header("Movement")]
    [Tooltip("Дистанция от цели для переключения на бег")]
    public float RunDistanceThreshold;
    
    [Header("Stuck Detection")]
    [Tooltip("Время для проверки застревания (секунды)")]
    public float StuckDetectionTime;
    
    [Tooltip("Минимальное расстояние прогресса для не-застревания")]
    public float MinProgressDistance;
    
    [Header("Combat")]
    [Tooltip("Дистанция атаки")]
    public float AttackRange;
    
    [Tooltip("Минимальный dot product угла для атаки (0.866 ≈ 30°)")]
    public float AttackAngleDotThreshold;
    public float LoseInterestTime;
    public float MaxChaseDistance;
    
    public static readonly NavMeshSettings Default = new()
    {
        AutoEnableOnStart = true,
        PathUpdateInterval = 0.5f,
        WaypointReachDistance = 1.5f,
        FinalDestinationDistance = 2f,
        PathRecalculationThreshold = 2f,
        NavMeshSampleDistance = 10f,
        RotationSpeed = 5f,
        IdleRotationSpeed = 3f,
        AttackRotationSpeed = 8f,
        StuckDetectionTime = 2f,
        MinProgressDistance = 0.5f,
        RunDistanceThreshold = 6f,
        AttackRange = 2.5f,
        AttackAngleDotThreshold = 0.866f, // ~30 градусов
        LoseInterestTime = 20,
        MaxChaseDistance = 50
    };
}