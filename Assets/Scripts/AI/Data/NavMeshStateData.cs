using UnityEngine;
using UnityEngine.AI;

public struct NavMeshStateData
{
    public Transform Transform;
    public Transform TargetTransform;
    public NavMeshPath NavMeshPath;
    public NavMeshSettings Settings;
    
    public int CurrentWaypointIndex;
    public float NextPathUpdateTime;
    public Vector3 LastTargetPosition;
    public Vector3 LastCharacterPosition;
    public float LastProgressCheckTime;
    public float LastDistanceToTarget;
    public bool IsEnabled;
    public bool IsStuck;
    public bool HasReachedDestination; 
    public bool IsRunning;
}

public struct NavMeshSettings
{
    public bool AutoEnableOnStart;
    public float PathUpdateInterval;
    public float WaypointReachDistance;
    public float FinalDestinationDistance;
    public float PathRecalculationThreshold;
    public float NavMeshSampleDistance;
    public float RotationSpeed;
    public float IdleRotationSpeed;
    public float StuckDetectionTime;
    public float MinProgressDistance;
    public float RunDistanceThreshold;
    
    public static readonly NavMeshSettings Default = new ()
    {
        AutoEnableOnStart = true,
        PathUpdateInterval = 0.5f,
        WaypointReachDistance = 1.5f,
        FinalDestinationDistance = 2f,
        PathRecalculationThreshold = 2f,
        NavMeshSampleDistance = 10f,
        RotationSpeed = 5f,
        IdleRotationSpeed = 3f,
        StuckDetectionTime = 2f,
        MinProgressDistance = 0.5f,
        RunDistanceThreshold = 6f
    };
}
