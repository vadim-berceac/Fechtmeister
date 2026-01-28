using System;
using Unity.Behavior;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Follow Target",
    story: "Follow [Target] and attack when in range",
    category: "Action/Combat",
    id: "f1a2b3c4d5e6f7g8h9i0j1k2l3m4n5o6"
)]
public partial class FollowTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> CurrentTarget;
    [SerializeReference] public BlackboardVariable<float> AttackRange;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed;

    private BehaviorNewInput _inputSystem;
    private Transform _selfTransform;

    protected override Status OnStart()
    {
        if (_inputSystem == null)
            _inputSystem = GameObject.GetComponent<BehaviorNewInput>();

        if (_selfTransform == null)
            _selfTransform = GameObject.transform;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_inputSystem == null || !_inputSystem.IsEnabled)
            return Status.Failure;

        if (CurrentTarget.Value == null)
        {
            _inputSystem.SimulateMove(Vector2.zero);
            return Status.Failure;
        }

        Transform targetTransform = CurrentTarget.Value.transform;
        Vector3 currentPos = _selfTransform.position;
        Vector3 targetPos = targetTransform.position;

        float distance = Vector3.Distance(currentPos, targetPos);

        if (distance <= AttackRange.Value)
        {
            _inputSystem.SimulateMove(Vector2.zero);
            return Status.Success;
        }

        Vector3 direction = (targetPos - currentPos).normalized;
        Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;

        if (horizontalDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
            _selfTransform.rotation = Quaternion.Slerp(
                _selfTransform.rotation,
                targetRotation,
                RotationSpeed.Value * Time.deltaTime
            );
        }

        Vector2 moveInput = Vector2.up * MoveSpeed.Value;
        _inputSystem.SimulateMove(moveInput);

        Vector3 euler = _selfTransform.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        _selfTransform.eulerAngles = euler;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_inputSystem != null)
            _inputSystem.SimulateMove(Vector2.zero);
    }
}

