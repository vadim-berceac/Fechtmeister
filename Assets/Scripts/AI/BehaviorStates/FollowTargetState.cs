using UnityEngine;
using UnityEngine.AI;

public struct FollowTargetState : INavMeshState
{
    public void Enter(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        ResetTrackingData(ref data);
        NavMeshUtility.CalculatePath(ref data);
        
        // Достаём оружие при преследовании цели
        if (!data.HasWeaponDrawn)
        {
            input.InvokeDrawWeapon();
            data.HasWeaponDrawn = true;
        }
    }

    public void Update(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        if (!ValidateTarget(ref data, input)) return;

        var distanceToTarget = Vector3.Distance(data.Transform.position, data.TargetTransform.position);
        
        // Управление бегом/ходьбой
        UpdateMovementSpeed(ref data, input, distanceToTarget);

        // Проверка достижения цели
        if (HasReachedDestination(ref data, input, distanceToTarget)) return;

        // Обнаружение застревания
        if (UpdateStuckDetection(ref data, input, distanceToTarget)) return;

        // Пересчёт пути при необходимости
        UpdatePathRecalculation(ref data);

        // Следование по пути
        if (ValidatePath(ref data))
        {
            NavMeshUtility.FollowPath(ref data, input);
        }
    }

    public void Exit(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        input.InvokeMove(Vector2.zero);
        input.SetRunState(false);
        data.IsStuck = false;
        data.HasReachedDestination = false;
    }

    private static bool ValidateTarget(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        if (data.TargetTransform != null) return true;
        
        input.InvokeMove(Vector2.zero);
        input.SetRunState(false);
        return false;
    }

    private static void UpdateMovementSpeed(ref NavMeshStateData data, NavMeshCharacterInput input, float distanceToTarget)
    {
        var shouldRun = distanceToTarget > data.Settings.RunDistanceThreshold;
        input.SetRunState(shouldRun);
    }

    private static bool HasReachedDestination(ref NavMeshStateData data, NavMeshCharacterInput input, float distanceToTarget)
    {
        // Проверка на уже достигнутую цель, которая отдалилась
        if (data.HasReachedDestination && distanceToTarget > data.Settings.FinalDestinationDistance * 1.5f)
        {
            ResetTrackingData(ref data);
            NavMeshUtility.CalculatePath(ref data);
            return false;
        }

        // Проверка приближения к цели
        if (distanceToTarget <= data.Settings.FinalDestinationDistance)
        {
            if (!data.HasReachedDestination)
            {
                data.HasReachedDestination = true;
                input.InvokeMove(Vector2.zero);
                input.SetRunState(false);
            }
            
            NavMeshUtility.RotateTowardsTarget(
                data.Transform, 
                data.TargetTransform.position, 
                data.Settings.IdleRotationSpeed
            );
            return true;
        }

        return false;
    }

    private static bool UpdateStuckDetection(ref NavMeshStateData data, NavMeshCharacterInput input, float distanceToTarget)
    {
        // Сброс состояния застревания если цель переместилась достаточно далеко
        if (data.IsStuck && distanceToTarget > data.Settings.FinalDestinationDistance * 2f)
        {
            ResetTrackingData(ref data);
            NavMeshUtility.CalculatePath(ref data);
            return false;
        }

        // Проверка прогресса движения
        if (Time.time - data.LastProgressCheckTime > data.Settings.StuckDetectionTime)
        {
            var progressDistance = Vector3.Distance(data.Transform.position, data.LastCharacterPosition);
            
            if (progressDistance < data.Settings.MinProgressDistance && 
                distanceToTarget > data.Settings.FinalDestinationDistance)
            {
                data.IsStuck = true;
                input.InvokeMove(Vector2.zero);
                input.SetRunState(false);
                Debug.LogWarning("[NavMeshInput] Character stuck! Waiting for target to move.");
                
                NavMeshUtility.RotateTowardsTarget(
                    data.Transform, 
                    data.TargetTransform.position, 
                    data.Settings.IdleRotationSpeed
                );
                return true;
            }
            
            data.LastCharacterPosition = data.Transform.position;
            data.LastProgressCheckTime = Time.time;
        }

        // Обработка состояния застревания
        if (data.IsStuck)
        {
            input.SetRunState(false);
            NavMeshUtility.RotateTowardsTarget(
                data.Transform, 
                data.TargetTransform.position, 
                data.Settings.IdleRotationSpeed
            );
            return true;
        }

        return false;
    }

    private static void UpdatePathRecalculation(ref NavMeshStateData data)
    {
        var shouldRecalculate = Time.time >= data.NextPathUpdateTime;

        // Проверка перемещения цели
        if (data.TargetTransform != null)
        {
            var targetMoved = Vector3.Distance(data.TargetTransform.position, data.LastTargetPosition);
            if (targetMoved > data.Settings.PathRecalculationThreshold)
            {
                shouldRecalculate = true;
            }
        }

        if (shouldRecalculate)
        {
            NavMeshUtility.CalculatePath(ref data);
            data.NextPathUpdateTime = Time.time + data.Settings.PathUpdateInterval;
        }
    }

    private static bool ValidatePath(ref NavMeshStateData data)
    {
        return data.NavMeshPath != null && 
               data.NavMeshPath.status == NavMeshPathStatus.PathComplete && 
               data.NavMeshPath.corners.Length > 0;
    }

    private static void ResetTrackingData(ref NavMeshStateData data)
    {
        data.LastCharacterPosition = data.Transform.position;
        data.LastProgressCheckTime = Time.time;
        data.CurrentWaypointIndex = 0;
        data.IsStuck = false;
        data.HasReachedDestination = false;
    }
}