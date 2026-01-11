using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtility
{
    public static void CalculatePath(ref NavMeshStateData data)
    {
        if (data.TargetTransform == null)
        {
            return;
        }

        var startPos = data.Transform.position;
        var targetPos = data.TargetTransform.position;

        var startFound = NavMesh.SamplePosition(startPos, out var startHit, data.Settings.NavMeshSampleDistance, NavMesh.AllAreas);
        var targetFound = NavMesh.SamplePosition(targetPos, out var targetHit, data.Settings.NavMeshSampleDistance, NavMesh.AllAreas);

        if (!startFound || !targetFound)
        {
            return;
        }

        var pathCalculated = NavMesh.CalculatePath(startHit.position, targetHit.position, NavMesh.AllAreas, data.NavMeshPath);
        
        if (!pathCalculated || data.NavMeshPath.status != NavMeshPathStatus.PathComplete)
        {
            return;
        }

        data.CurrentWaypointIndex = 0;
        data.LastTargetPosition = targetPos;
    }

    public static void FollowPath(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        if (data.TargetTransform != null)
        {
            var currentDistanceToTarget = Vector3.Distance(data.Transform.position, data.TargetTransform.position);
           
            if (currentDistanceToTarget <= data.Settings.FinalDestinationDistance)
            {
                input.InvokeMove(Vector2.zero);
                return;
            }
        }
        
        if (data.CurrentWaypointIndex >= data.NavMeshPath.corners.Length)
        {
            input.InvokeMove(Vector2.zero);
            return;
        }

        var targetWaypoint = data.NavMeshPath.corners[data.CurrentWaypointIndex];
        var horizontalPosition = new Vector3(data.Transform.position.x, 0, data.Transform.position.z);
        var horizontalWaypoint = new Vector3(targetWaypoint.x, 0, targetWaypoint.z);
        var distanceToWaypoint = Vector3.Distance(horizontalPosition, horizontalWaypoint);
        
        if (distanceToWaypoint < data.Settings.WaypointReachDistance)
        {
            data.CurrentWaypointIndex++;

            if (data.CurrentWaypointIndex >= data.NavMeshPath.corners.Length)
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

                CalculatePath(ref data);
                return;
            }

            targetWaypoint = data.NavMeshPath.corners[data.CurrentWaypointIndex];
           
            if (data.TargetTransform != null)
            {
                var distCheck = Vector3.Distance(data.Transform.position, data.TargetTransform.position);
                if (distCheck <= data.Settings.FinalDestinationDistance)
                {
                    input.InvokeMove(Vector2.zero);
                    return;
                }
            }
        }

        var directionWorld = targetWaypoint - data.Transform.position;
        directionWorld.y = 0;

        if (directionWorld.magnitude <= 0.01f)
        {
            input.InvokeMove(Vector2.zero);
            return;
        }

        directionWorld.Normalize();
        
        RotateTowardsTarget(data.Transform, targetWaypoint, data.Settings.RotationSpeed);

        var localDirection = data.Transform.InverseTransformDirection(directionWorld);
        var moveInput = new Vector2(localDirection.x, localDirection.z).normalized;

        input.InvokeMove(moveInput);
    }

    public static void RotateTowardsTarget(Transform transform, Vector3 targetPosition, float speed)
    {
        var directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0; 
        
        if (directionToTarget.magnitude < 0.01f)
        {
            return; 
        }
        
        directionToTarget.Normalize();
        var targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
    }
}
