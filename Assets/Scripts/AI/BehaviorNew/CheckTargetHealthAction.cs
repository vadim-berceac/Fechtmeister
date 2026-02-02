using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckTargetHealth", story: "Check if Target is Still Alive", category: "Action", id: "2361b15649263c171db4b6f5c8d46eed")]
public partial class CheckTargetHealthAction : Action
{
    [SerializeReference] public BlackboardVariable<HealthComponent> CurrentTarget;

    protected override Status OnStart()
    {
        if (CurrentTarget?.Value == null)
        {
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (CurrentTarget?.Value == null || CurrentTarget.Value.IsDestroyed)
        {
            CurrentTarget.Value = null;
            
            return Status.Failure;
        }

        return Status.Success;
    }
}