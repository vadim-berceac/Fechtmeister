using System.Collections.Generic;
using Unity.Behavior;
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
        var distance = (currentPos - targetPos).magnitude;
       
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
            var moveInput = config.IsRun ? Vector2.up : Vector2.up * 0.5f;
            config.InputSystem.SimulateMove(moveInput);
        }
        else
        {
            config.InputSystem.SimulateMove(Vector2.zero);
        }
        
        return Status.Running;
    }
    
    public static Status MoveDirectlyToTarget(
        this Action action,
        Transform selfTransform,
        Vector3 targetPosition,
        BehaviorNewInput inputSystem,
        float moveSpeed,
        float rotationSpeed,
        float stoppingDistance = 0f)
    {
        if (inputSystem == null || !inputSystem.IsEnabled)
            return Status.Failure;

        var currentPos = selfTransform.position;
        var distance = Vector3.Distance(currentPos, targetPosition);

        // Достигли цели
        if (stoppingDistance > 0 && distance <= stoppingDistance)
        {
            inputSystem.SimulateMove(Vector2.zero);
            return Status.Success;
        }

        var direction = (targetPosition - currentPos).normalized;
        var horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;

        if (horizontalDirection.magnitude > 0.1f)
        {
            var targetRotation = Quaternion.LookRotation(horizontalDirection);
            selfTransform.rotation = Quaternion.Slerp(
                selfTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        var moveInput = Vector2.up * moveSpeed;
        inputSystem.SimulateMove(moveInput);

        return Status.Running;
    }
    
    /// <summary>
    /// Вычисляет NavMesh путь между двумя точками
    /// </summary>
    public static bool TryCalculateNavMeshPath(
        this Action action,
        Vector3 from,
        Vector3 to,
        out List<Vector3> waypoints,
        int areaMask = NavMesh.AllAreas)
    {
        waypoints = null;
        var path = new NavMeshPath();
        
        if (!NavMesh.CalculatePath(from, to, areaMask, path))
        {
           return false;
        }
        
        if (path.status != NavMeshPathStatus.PathComplete)
        {
           return false;
        }
        waypoints = new List<Vector3>(path.corners);
        return true;
    }
    
    /// <summary>
    /// Проверяет нужно ли пересчитать путь до движущейся цели
    /// </summary>
    public static bool ShouldRecalculatePath(
        float timeSinceLastRecalculation,
        float recalculationInterval,
        Vector3 currentTargetPosition,
        Vector3 lastTargetPosition,
        float movementThreshold = 2f)
    {
        var timeElapsed = timeSinceLastRecalculation >= recalculationInterval;
        var targetMoved = (currentTargetPosition - lastTargetPosition).magnitude > movementThreshold;
        
        return timeElapsed || targetMoved;
    }
    
    /// <summary>
    /// Проверяет удяляется ли цель
    /// </summary>
    public static bool CheckTargetLeaves(this Action action, BlackboardVariable<HealthComponent> currentTarget,
        Transform selfTransform, BlackboardVariable<float> attackRange, ref Vector3 lastTargetPosition,
        PathFollowingState pathState, float targetMovementThreshold = 0.1f)
    {
        if (currentTarget.Value == null)
            return false;

        var currentPos = selfTransform.position;
        var targetPos = currentTarget.Value.transform.position;
        var currentDistance = (currentPos - targetPos).magnitude;

        if (currentDistance <= attackRange.Value)
            return false;

        if (lastTargetPosition == Vector3.zero)
        {
            lastTargetPosition = targetPos;
            return false;
        }

        var targetMovementDelta = (targetPos - lastTargetPosition).magnitude;
        var targetIsStopped = targetMovementDelta < targetMovementThreshold;

        if (targetIsStopped)
            return false;

        var previousDistance = (pathState.LastPosition - lastTargetPosition).magnitude;
        var targetIsMovingAway = currentDistance > previousDistance;

        return targetIsMovingAway;
    }
    
    /// <summary>
    /// Проверяет нужно ли бежать за целью
    /// </summary>
    public static bool CheckIfNeedToRun(this Action action, Vector3 currentPos, Vector3 targetPos, float currentDistance,
        float attackRange, ref Vector3 previousTargetPosition, ref float previousCheckTime,
        bool isRunning, BlackboardVariable<float> targetSpeedThreshold)
    {
        if (currentDistance <= attackRange)
            return false;

        if (previousTargetPosition == Vector3.zero)
        {
            previousTargetPosition = targetPos;
            previousCheckTime = Time.time;
            return false;
        }

        var deltaTime = Time.time - previousCheckTime;
        if (deltaTime < 0.1f) 
            return isRunning; 

        var targetMovement = (targetPos - previousTargetPosition).magnitude;
        var targetSpeed = targetMovement / deltaTime;

        var previousDistance = (currentPos - previousTargetPosition).magnitude;
        var targetMovingAway = currentDistance > previousDistance + 0.1f; 

        previousTargetPosition = targetPos;
        previousCheckTime = Time.time;

        bool needRun = targetSpeed > targetSpeedThreshold.Value && targetMovingAway;

        return needRun;
    }

    public static float GetAttackRange(this Action action, CharacterCore character)
    {
        return character.Inventory.WeaponSystem.InstanceInHands == null
            ? 2.5f
            : ((WeaponData)character.Inventory.WeaponSystem.InstanceInHands.EquppiedItemData).WeaponParams
            .PreferredDistance;
    }

    public static bool IsWeaponRanged(this Action action, CharacterCore character)
    {
        if (character.Inventory.WeaponSystem.InstanceInHands == null)
        {
            return false;
        }
        return ((WeaponData)character.Inventory.WeaponSystem.InstanceInHands.EquppiedItemData).RangeType != RangeTypes.Melee;
    }
}
