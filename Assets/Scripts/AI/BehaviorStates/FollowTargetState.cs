using UnityEngine;

public struct FollowTargetState : INavMeshState
{
    public void Enter(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        data.LastCharacterPosition = data.Transform.position;
        data.LastProgressCheckTime = Time.time;
        data.LastDistanceToTarget = float.MaxValue;
        data.CurrentWaypointIndex = 0;
        data.IsStuck = false;
        data.HasReachedDestination = false;
        
        NavMeshUtility.CalculatePath(ref data);
    }

    public void Update(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        if (data.TargetTransform == null)
        {
            input.InvokeMove(Vector2.zero);
            input.SetRunState(false);
            return;
        }

        var distanceToTarget = Vector3.Distance(data.Transform.position, data.TargetTransform.position);

        var shouldRun = distanceToTarget > data.Settings.RunDistanceThreshold;
        input.SetRunState(shouldRun);

        if (data.HasReachedDestination && distanceToTarget > data.Settings.FinalDestinationDistance * 1.5f)
        {
            data.HasReachedDestination = false;
            data.IsStuck = false;
            data.LastCharacterPosition = data.Transform.position;
            data.LastProgressCheckTime = Time.time;
            data.LastDistanceToTarget = float.MaxValue;
            NavMeshUtility.CalculatePath(ref data);
        }

        if (data.IsStuck && distanceToTarget > data.Settings.FinalDestinationDistance * 2f)
        {
            data.IsStuck = false;
            data.LastCharacterPosition = data.Transform.position;
            data.LastProgressCheckTime = Time.time;
            data.LastDistanceToTarget = float.MaxValue;
            NavMeshUtility.CalculatePath(ref data);
        }

        if (distanceToTarget <= data.Settings.FinalDestinationDistance * 2f && 
            distanceToTarget > data.LastDistanceToTarget && 
            data.LastDistanceToTarget <= data.Settings.FinalDestinationDistance * 1.2f)
        {
            data.HasReachedDestination = true;
            input.InvokeMove(Vector2.zero);
            input.SetRunState(false);
         
            NavMeshUtility.RotateTowardsTarget(
                data.Transform, 
                data.TargetTransform.position, 
                data.Settings.IdleRotationSpeed
            );
            return;
        }
        
        data.LastDistanceToTarget = distanceToTarget;

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
            return;
        }

        if (Time.time - data.LastProgressCheckTime > data.Settings.StuckDetectionTime)
        {
            var progressDistance = Vector3.Distance(data.Transform.position, data.LastCharacterPosition);
          
            if (progressDistance < data.Settings.MinProgressDistance && 
                distanceToTarget > data.Settings.FinalDestinationDistance)
            {
                data.IsStuck = true;
                input.InvokeMove(Vector2.zero);
                input.SetRunState(false);
                Debug.LogWarning($"[NavMeshInput] Character stuck! Will retry if target moves.");
             
                NavMeshUtility.RotateTowardsTarget(
                    data.Transform, 
                    data.TargetTransform.position, 
                    data.Settings.IdleRotationSpeed
                );
                return;
            }
            
            data.LastCharacterPosition = data.Transform.position;
            data.LastProgressCheckTime = Time.time;
        }

        if (data.IsStuck)
        {
            input.SetRunState(false);
            NavMeshUtility.RotateTowardsTarget(
                data.Transform, 
                data.TargetTransform.position, 
                data.Settings.IdleRotationSpeed
            );
            return;
        }

        var shouldRecalculate = Time.time >= data.NextPathUpdateTime;

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

        if (data.NavMeshPath == null || 
            data.NavMeshPath.status != UnityEngine.AI.NavMeshPathStatus.PathComplete || 
            data.NavMeshPath.corners.Length == 0)
        {
            return;
        }

        NavMeshUtility.FollowPath(ref data, input);
    }

    public void Exit(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        input.InvokeMove(Vector2.zero);
        data.IsStuck = false;
        data.HasReachedDestination = false;
    }
}