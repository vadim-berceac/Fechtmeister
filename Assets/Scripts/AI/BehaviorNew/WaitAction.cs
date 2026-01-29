using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitAction", story: "Wait", category: "Action/Movement", id: "1f3c9e653f97b4ed45815493b145cbc2")]
public partial class WaitAction : Action
{
    [SerializeReference] public BlackboardVariable<float> WaitDuration;
    [SerializeReference] public BlackboardVariable<BehaviorNewInput> InputSystem;
    
    private float _elapsedTime;

    protected override Status OnStart()
    {
        Debug.Log("[WaitAction] Activated after Restart");
        _elapsedTime = 0f;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (InputSystem.Value == null || !InputSystem.Value.IsEnabled)
            return Status.Failure;
        
        if (InputSystem.Value.IsInCombatMode)
        {
            InputSystem.Value.SimulateMove(Vector2.zero);
            Debug.Log("<color=yellow>[MoveToPoint] Combat started, aborting patrol</color>");
            return Status.Failure;
        }
        
        _elapsedTime += Time.deltaTime;
        
        if (_elapsedTime >= WaitDuration.Value)
            return Status.Success;
            
        return Status.Running;
    }
}

