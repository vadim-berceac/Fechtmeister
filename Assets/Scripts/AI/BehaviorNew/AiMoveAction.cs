using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AiMove", story: "Agent Move", category: "Action/Movement", id: "08148b4f8321e78a654e0bcb1db9db13")]
public partial class AiMoveAction : Action
{
    [SerializeReference] public BlackboardVariable<BehaviorNewInput> InputSystem;
    [SerializeReference] public BlackboardVariable<Vector2> MoveDirection;

    protected override Status OnStart()
    {
        if (InputSystem.Value != null && InputSystem.Value.IsEnabled)
        {
            InputSystem.Value.SimulateMove(MoveDirection.Value);
            return Status.Success;
        }
        return Status.Failure;
    }
}