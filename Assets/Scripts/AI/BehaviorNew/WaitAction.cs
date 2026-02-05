using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitAction", story: "Wait", category: "Action/Movement", id: "1f3c9e653f97b4ed45815493b145cbc2")]
public partial class WaitAction : Action
{
    [SerializeReference] public BlackboardVariable<HealthComponent> CurrentTarget;
    [SerializeReference] public BlackboardVariable<float> MinWaitDuration;
    [SerializeReference] public BlackboardVariable<float> MaxWaitDuration;
    [SerializeReference] public BlackboardVariable<BehaviorNewInput> InputSystem;
    
    private float _waitDuration;
    private float _elapsedTime;

    protected override Status OnStart()
    {
        _waitDuration = Random.Range(MinWaitDuration.Value, MaxWaitDuration.Value);
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
        
        if (_elapsedTime >= _waitDuration || CurrentTarget.Value != null)
        {
            return Status.Success;
        }

        return Status.Running;
    }
}

