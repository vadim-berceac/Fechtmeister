using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetRandomNavMeshPointAction", story: "Get Random point on NavMesh",
    category: "Action/Navigation", id: "a971b051503e229cf3e616d9bee0843c")]
public partial class GetRandomNavMeshPointAction : Action
{ 
    [SerializeReference] public BlackboardVariable<Transform> SelfTransform;
    [SerializeReference] public BlackboardVariable<float> WanderRadius;
    [SerializeReference] public BlackboardVariable<Vector3> TargetPosition;
    [SerializeReference] public BlackboardVariable<BehaviorNewInput> InputSystem;

    protected override Status OnStart()
    {
        Vector3 randomPoint = GetRandomNavMeshPoint(
            SelfTransform.Value.position, 
            WanderRadius.Value
        );
        
        if (randomPoint == Vector3.zero)
            return Status.Failure;
            
        TargetPosition.Value = randomPoint;
        return Status.Success;
    }
    
    private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += center;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            {
                // Проверяем, что персонаж может достичь эту точку
                if (IsPointReachable(center, hit.position))
                {
                    return hit.position;
                }
            }
        }
        return Vector3.zero;
    }
    
    private bool IsPointReachable(Vector3 from, Vector3 to)
    {
        NavMeshPath path = new NavMeshPath();
        
        if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        
        return false;
    }
}