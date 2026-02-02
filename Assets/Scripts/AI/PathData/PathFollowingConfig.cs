using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Конфигурация для следования по пути
/// </summary>
public struct PathFollowingConfig
{
    public List<Vector3> Waypoints;
    public Transform SelfTransform;
    public BehaviorNewInput InputSystem;
    public float StoppingDistance;
    public bool IsRun;
    public float RotationSpeed;
    public float MaxRotationBeforeMove;
    public float TimeoutDuration;
}
