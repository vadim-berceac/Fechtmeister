using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtility
{
    // Кэшируем хиты для избежания аллокаций
    private static NavMeshHit _cachedStartHit;
    private static NavMeshHit _cachedTargetHit;

    public static void CalculatePath(ref NavMeshStateData data)
    {
        if (data.TargetTransform == null) return;

        var startPos = data.Transform.position;
        var targetPos = data.TargetTransform.position;

        // Sample позиции на NavMesh
        var startFound = NavMesh.SamplePosition(
            startPos, 
            out _cachedStartHit, 
            data.Settings.NavMeshSampleDistance, 
            NavMesh.AllAreas
        );
        
        var targetFound = NavMesh.SamplePosition(
            targetPos, 
            out _cachedTargetHit, 
            data.Settings.NavMeshSampleDistance, 
            NavMesh.AllAreas
        );

        if (!startFound || !targetFound) return;

        // Вычисляем путь
        var pathCalculated = NavMesh.CalculatePath(
            _cachedStartHit.position, 
            _cachedTargetHit.position, 
            NavMesh.AllAreas, 
            data.NavMeshPath
        );
        
        if (!pathCalculated || data.NavMeshPath.status != NavMeshPathStatus.PathComplete) return;

        data.CurrentWaypointIndex = 0;
        data.LastTargetPosition = targetPos;
    }

    public static void FollowPath(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        // Ранний выход если достигли конца пути
        if (data.CurrentWaypointIndex >= data.NavMeshPath.corners.Length)
        {
            input.InvokeMove(Vector2.zero);
            return;
        }

        // Проверка финальной дистанции
        if (data.TargetTransform != null)
        {
            var currentDistanceToTarget = Vector3.Distance(
                data.Transform.position, 
                data.TargetTransform.position
            );
           
            if (currentDistanceToTarget <= data.Settings.FinalDestinationDistance)
            {
                input.InvokeMove(Vector2.zero);
                return;
            }
        }

        var targetWaypoint = data.NavMeshPath.corners[data.CurrentWaypointIndex];
        
        // Проверка достижения текущей точки маршрута (игнорируем Y)
        if (IsWaypointReached(data, targetWaypoint))
        {
            data.CurrentWaypointIndex++;

            if (data.CurrentWaypointIndex >= data.NavMeshPath.corners.Length)
            {
                HandlePathEnd(ref data, input);
                return;
            }

            targetWaypoint = data.NavMeshPath.corners[data.CurrentWaypointIndex];
        }

        // Движение к точке
        MoveTowardsWaypoint(ref data, input, targetWaypoint);
    }

    private static bool IsWaypointReached(NavMeshStateData data, Vector3 waypoint)
    {
        var horizontalPosition = new Vector3(data.Transform.position.x, 0, data.Transform.position.z);
        var horizontalWaypoint = new Vector3(waypoint.x, 0, waypoint.z);
        var distanceToWaypoint = Vector3.Distance(horizontalPosition, horizontalWaypoint);
        
        return distanceToWaypoint < data.Settings.WaypointReachDistance;
    }

    private static void HandlePathEnd(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        if (data.TargetTransform == null)
        {
            input.InvokeMove(Vector2.zero);
            return;
        }

        var finalDistance = Vector3.Distance(data.Transform.position, data.TargetTransform.position);
        
        if (finalDistance <= data.Settings.FinalDestinationDistance)
        {
            input.InvokeMove(Vector2.zero);
            return;
        }

        // Пересчитываем путь если не достигли цели
        CalculatePath(ref data);
    }

    private static void MoveTowardsWaypoint(ref NavMeshStateData data, NavMeshCharacterInput input, Vector3 targetWaypoint)
    {
        var directionWorld = targetWaypoint - data.Transform.position;
        directionWorld.y = 0;

        if (directionWorld.sqrMagnitude <= 0.0001f) // Используем sqrMagnitude для оптимизации
        {
            input.InvokeMove(Vector2.zero);
            return;
        }

        directionWorld.Normalize();
        
        // Поворачиваем персонажа
        RotateTowardsTarget(data.Transform, targetWaypoint, data.Settings.RotationSpeed);

        // Преобразуем в локальное направление для input системы
        var localDirection = data.Transform.InverseTransformDirection(directionWorld);
        var moveInput = new Vector2(localDirection.x, localDirection.z).normalized;

        input.InvokeMove(moveInput);
    }

    public static void RotateTowardsTarget(Transform transform, Vector3 targetPosition, float speed)
    {
        var directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0;
        
        if (directionToTarget.sqrMagnitude < 0.0001f) return;
        
        directionToTarget.Normalize();
        var targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
    }
}