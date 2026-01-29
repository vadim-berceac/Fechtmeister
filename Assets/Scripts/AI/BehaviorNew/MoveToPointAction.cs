using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToPointAction", story: "Move to point", category: "Action/Movement", id: "04ff221d32a60605efc6e9de9a07558e")]
public partial class MoveToPointAction : Action
{
    [SerializeReference] public BlackboardVariable<BehaviorNewInput> InputSystem;
    [SerializeReference] public BlackboardVariable<Transform> SelfTransform;
    [SerializeReference] public BlackboardVariable<Vector3> TargetPosition;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed = new BlackboardVariable<float>(5f);
    [SerializeReference] public BlackboardVariable<float> MaxRotationBeforeMove = new BlackboardVariable<float>(45f);
    [SerializeReference] public BlackboardVariable<float> TimeoutDuration = new BlackboardVariable<float>(1f);

    private float _timeElapsed;
    private Vector3 _lastPosition;
    private float _stuckTime;

    protected override Status OnStart()
    {
        Debug.Log("[MoveToPointAction] Activated after Restart");
        _timeElapsed = 0f;
        _stuckTime = 0f;
        _lastPosition = SelfTransform.Value.position;
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

        _timeElapsed += Time.deltaTime;

        Vector3 currentPos = SelfTransform.Value.position;
        Vector3 targetPos = TargetPosition.Value;
       
        float distance = Vector3.Distance(currentPos, targetPos);
        if (distance <= StoppingDistance.Value)
        {
            InputSystem.Value.SimulateMove(Vector2.zero);
            return Status.Success;
        }

        float movedDistance = Vector3.Distance(currentPos, _lastPosition);
        if (movedDistance < 0.01f) // Почти не движется
        {
            _stuckTime += Time.deltaTime;
            if (_stuckTime >= TimeoutDuration.Value)
            {
                InputSystem.Value.SimulateMove(Vector2.zero);
                Debug.LogWarning($"MoveToPoint: Stuck for {TimeoutDuration.Value}s, aborting");
                return Status.Failure;
            }
        }
        else
        {
            _stuckTime = 0f; 
        }
        _lastPosition = currentPos;

        Vector3 direction = (targetPos - currentPos).normalized;
       
        Vector3 forward = SelfTransform.Value.forward;
        float angleToTarget = Vector3.Angle(forward, direction);

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            SelfTransform.Value.rotation = Quaternion.Slerp(
                SelfTransform.Value.rotation, 
                targetRotation, 
                RotationSpeed.Value * Time.deltaTime
            );
        }

        if (angleToTarget < MaxRotationBeforeMove.Value)
        {
            Vector2 moveInput = Vector2.up * MoveSpeed.Value;
            InputSystem.Value.SimulateMove(moveInput);
        }
        else
        {
            InputSystem.Value.SimulateMove(Vector2.zero);
        }
        
        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (InputSystem.Value != null)
            InputSystem.Value.SimulateMove(Vector2.zero);
    }
}