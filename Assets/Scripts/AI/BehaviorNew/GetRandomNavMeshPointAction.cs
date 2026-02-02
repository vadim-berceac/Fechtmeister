using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetRandomNavMeshPointAction", story: "Get Random point on NavMesh",
    category: "Action/Navigation", id: "a971b051503e229cf3e616d9bee0843c")]
public partial class GetRandomNavMeshPointAction : Action
{ 
    [SerializeReference] public BlackboardVariable<Transform> SelfTransform;
    [SerializeReference] public BlackboardVariable<float> WanderRadius;
    [SerializeReference] public BlackboardVariable<List<Vector3>> Waypoints;
    [SerializeReference] public BlackboardVariable<BehaviorNewInput> InputSystem;

    protected override Status OnStart()
    {
        var path = this.GetRandomNavMeshPath(SelfTransform.Value.position, WanderRadius.Value,3);
        
        if (path == null || path.Count == 0)
            return Status.Failure;
            
        Waypoints.Value = path;
        return Status.Success;
    }
}