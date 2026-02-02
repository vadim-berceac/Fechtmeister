using UnityEngine;

/// <summary>
/// Состояние для следования по пути (ref параметры)
/// </summary>
public struct PathFollowingState
{
    public int CurrentWaypointIndex;
    public Vector3 LastPosition;
    public float StuckTime;
}