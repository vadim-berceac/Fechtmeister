using System;
using System.Collections.Generic;
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
    [SerializeReference] public BlackboardVariable<List<Vector3>> Waypoints; // List вместо массива
    [SerializeReference] public BlackboardVariable<float> MoveSpeed;
    [SerializeReference] public BlackboardVariable<float> StoppingDistance;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed = new BlackboardVariable<float>(5f);
    [SerializeReference] public BlackboardVariable<float> MaxRotationBeforeMove = new BlackboardVariable<float>(45f);
    [SerializeReference] public BlackboardVariable<float> TimeoutDuration = new BlackboardVariable<float>(1f);

    private Vector3 _lastPosition;
    private float _stuckTime;
    private int _currentWaypointIndex;
    private PathFollowingState _state;
    
    protected override Status OnStart()
    {
        _state = new PathFollowingState
        {
            CurrentWaypointIndex = 0,
            LastPosition = SelfTransform.Value.position,
            StuckTime = 0f
        };
       
        if (Waypoints.Value == null || Waypoints.Value.Count == 0)
        {
            Debug.LogError("[MoveToPointAction] No waypoints provided!");
            return Status.Failure;
        }
     
        if (Waypoints.Value.Count > 1)
        {
            var distToFirst = (SelfTransform.Value.position - Waypoints.Value[0]).magnitude;
            if (distToFirst < StoppingDistance.Value)
            {
                _state.CurrentWaypointIndex = 1;
            }
        }
        
        return Status.Running;
    }
    
    protected override Status OnUpdate()
    {
        var config = new PathFollowingConfig
        {
            Waypoints = Waypoints.Value,
            SelfTransform = SelfTransform.Value,
            InputSystem = InputSystem.Value,
            StoppingDistance = StoppingDistance.Value,
            MoveSpeed = MoveSpeed.Value,
            RotationSpeed = RotationSpeed.Value,
            MaxRotationBeforeMove = MaxRotationBeforeMove.Value,
            TimeoutDuration = TimeoutDuration.Value
        };

        return this.FollowPath(config, ref _state);
    }

    protected override void OnEnd()
    {
        if (InputSystem.Value != null)
            InputSystem.Value.SimulateMove(Vector2.zero);
    }
}