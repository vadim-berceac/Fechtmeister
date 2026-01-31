using System;
using System.Collections.Generic;
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
    [SerializeReference] public BlackboardVariable<BehaviorNewInput> InputSystem;
    [SerializeReference] public BlackboardVariable<HealthComponent> CurrentTarget;
    [SerializeReference] public BlackboardVariable<float> AttackRange;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed;
    [SerializeReference] public BlackboardVariable<float> PathRecalculateInterval = new BlackboardVariable<float>(0.5f);
    [SerializeReference] public BlackboardVariable<float> StoppingDistance = new BlackboardVariable<float>(0.5f);
    [SerializeReference] public BlackboardVariable<float> MaxRotationBeforeMove = new BlackboardVariable<float>(45f);
    [SerializeReference] public BlackboardVariable<float> TimeoutDuration = new BlackboardVariable<float>(2f);
    [SerializeReference] public BlackboardVariable<float> TargetMovementThreshold = new BlackboardVariable<float>(2f);

    private Transform _selfTransform;
    private List<Vector3> _currentPath;
    private PathFollowingState _pathState;
    private float _lastPathRecalculateTime;
    private Vector3 _lastTargetPosition;
    private bool _needToRun;
    private bool _isRunning;

    protected override Status OnStart()
    {
        if (_selfTransform == null)
            _selfTransform = GameObject.transform;

        _currentPath = new List<Vector3>();
        _pathState = new PathFollowingState
        {
            CurrentWaypointIndex = 0,
            LastPosition = _selfTransform.position,
            StuckTime = 0f
        };
        _lastPathRecalculateTime = -PathRecalculateInterval.Value;
        _lastTargetPosition = Vector3.zero;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (InputSystem == null || !InputSystem.Value.IsEnabled)
            return Status.Failure;

        if (CurrentTarget.Value == null)
        {
            InputSystem.Value.SimulateMove(Vector2.zero);
            return Status.Failure;
        }

        var targetTransform = CurrentTarget.Value.transform;
        var currentPos = _selfTransform.position;
        var targetPos = targetTransform.position;
        var distance = (currentPos - targetPos).magnitude;
        
        _needToRun = this.CheckTargetLeaves(CurrentTarget, _selfTransform, AttackRange,
            ref _lastTargetPosition, _pathState);

        if (_needToRun && !_isRunning)
        {
            _isRunning = true;
            InputSystem.Value.SwitchRunMode();
        }
        // else if (!_needToRun && _isRunning)
        // {
        //     _isRunning = false;
        //     InputSystem.Value.SwitchRunMode();
        // }

        if (distance <= AttackRange.Value)
        {
            InputSystem.Value.SimulateMove(Vector2.zero);
            return Status.Success;
        }

        var timeSinceLastRecalc = Time.time - _lastPathRecalculateTime;
        var needRecalculate = ActionExtensions.ShouldRecalculatePath(
            timeSinceLastRecalc,
            PathRecalculateInterval.Value,
            targetPos,
            _lastTargetPosition,
            TargetMovementThreshold.Value
        );

        if (needRecalculate || _currentPath.Count == 0)
        {
            if (this.TryCalculateNavMeshPath(currentPos, targetPos, out _currentPath))
            {
                _lastPathRecalculateTime = Time.time;
                _lastTargetPosition = targetPos;
                _pathState.CurrentWaypointIndex = 0;
                _pathState.StuckTime = 0f;
            }
            else
            {
                return this.MoveDirectlyToTarget(
                    _selfTransform,
                    targetPos,
                    InputSystem.Value,
                    MoveSpeed.Value,
                    RotationSpeed.Value
                );
            }
        }

        var config = new PathFollowingConfig
        {
            Waypoints = _currentPath,
            SelfTransform = _selfTransform,
            InputSystem = InputSystem.Value,
            StoppingDistance = StoppingDistance.Value,
            IsRun = _needToRun,
            RotationSpeed = RotationSpeed.Value,
            MaxRotationBeforeMove = MaxRotationBeforeMove.Value,
            TimeoutDuration = TimeoutDuration.Value
        };

        var pathStatus = this.FollowPath(config, ref _pathState);

        if (pathStatus == Status.Success && distance > AttackRange.Value)
        {
            return Status.Running;
        }

        return pathStatus;
    }
    
    protected override void OnEnd()
    {
        if (InputSystem.Value != null)
            InputSystem.Value.SimulateMove(Vector2.zero);
    }
}