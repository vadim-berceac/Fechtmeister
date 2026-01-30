using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Status = Unity.Behavior.Node.Status;

public static class ActionExtensions
{
    public static List<Vector3> GetRandomNavMeshPath(this Action action, Vector3 center, float radius, int iterations)
    {
        for (var i = 0; i < iterations; i++)
        {
            var randomDirection = Random.insideUnitSphere * radius;
            randomDirection += center;
           
            if (!NavMesh.SamplePosition(randomDirection, out var hit, radius, NavMesh.AllAreas))
            {
                return null;
            }
            
            var path = new NavMeshPath();
                
            if (!NavMesh.CalculatePath(center, hit.position, NavMesh.AllAreas, path))
            {
                return null;
            }
            
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return new List<Vector3>(path.corners);
            }
        }
        return null;
    }
    
    public static Status FollowPath(
        this Action action,
        PathFollowingConfig config,
        ref PathFollowingState state)
    {
        if (config.InputSystem == null || !config.InputSystem.IsEnabled)
            return Status.Failure;

        if (config.Waypoints == null || config.Waypoints.Count == 0)
            return Status.Failure;

        var currentPos = config.SelfTransform.position;
        
        // Достигли конца пути
        if (state.CurrentWaypointIndex >= config.Waypoints.Count)
        {
            config.InputSystem.SimulateMove(Vector2.zero);
            return Status.Success;
        }
        
        var targetPos = config.Waypoints[state.CurrentWaypointIndex];
        var distance = Vector3.Distance(currentPos, targetPos);
       
        // Достигли текущего waypoint
        if (distance <= config.StoppingDistance)
        {
            state.CurrentWaypointIndex++;
            
            if (state.CurrentWaypointIndex >= config.Waypoints.Count)
            {
                config.InputSystem.SimulateMove(Vector2.zero);
                return Status.Success;
            }
            
            targetPos = config.Waypoints[state.CurrentWaypointIndex];
        }

        // Проверка на застревание
        var movedDistance = (currentPos - state.LastPosition).magnitude;
        if (movedDistance < 0.01f)
        {
            state.StuckTime += Time.deltaTime;
            if (state.StuckTime >= config.TimeoutDuration)
            {
                config.InputSystem.SimulateMove(Vector2.zero);
                Debug.LogWarning($"[FollowPath] Stuck at waypoint {state.CurrentWaypointIndex}, aborting");
                return Status.Failure;
            }
        }
        else
        {
            state.StuckTime = 0f; 
        }
        state.LastPosition = currentPos;

        // Поворот к цели
        var direction = (targetPos - currentPos).normalized;
        var forward = config.SelfTransform.forward;
        var angleToTarget = Vector3.Angle(forward, direction);

        if (direction.magnitude > 0.1f)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            config.SelfTransform.rotation = Quaternion.Slerp(
                config.SelfTransform.rotation, 
                targetRotation, 
                config.RotationSpeed * Time.deltaTime
            );
        }

        // Движение если повернуты достаточно
        if (angleToTarget < config.MaxRotationBeforeMove)
        {
            var moveInput = Vector2.up * config.MoveSpeed;
            config.InputSystem.SimulateMove(moveInput);
        }
        else
        {
            config.InputSystem.SimulateMove(Vector2.zero);
        }
        
        return Status.Running;
    }
}
