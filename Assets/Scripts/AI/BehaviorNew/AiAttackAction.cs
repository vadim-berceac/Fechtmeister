using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AIAttackAction", story: "Agent attacks Target", category: "Action/Combat", id: "746b0e7eb5bc99ac2df26cdf048549c9")]
public partial class AiAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<BehaviorNewInput> InputSystem;

    protected override Status OnStart()
    {
        if (InputSystem.Value != null && InputSystem.Value.IsEnabled)
        {
            InputSystem.Value.SimulateAttack();
            return Status.Success;
        }
        return Status.Failure;
    }

}