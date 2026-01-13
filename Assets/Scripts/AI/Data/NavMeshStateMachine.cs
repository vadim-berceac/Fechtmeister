using UnityEngine;

public enum NavMeshStateType
{
    Idle,
    Follow,
    Attack
}

public class NavMeshStateMachine
{
    private readonly NavMeshCharacterInput _input;
    private INavMeshState _currentState;
    private NavMeshStateType _currentStateType;

    private readonly IdleBehaviorState _idleState;
    private readonly FollowTargetState _followState;
    private readonly AttackTargetState _attackState;

    public NavMeshStateMachine(NavMeshCharacterInput input)
    {
        _input = input;
        
        // Кэшируем состояния для избежания аллокаций
        _idleState = new IdleBehaviorState();
        _followState = new FollowTargetState();
        _attackState = new AttackTargetState();
    }

    public void Update(ref NavMeshStateData stateData)
    {
        _currentState?.Update(ref stateData, _input);
        
        // Автоматические переходы между состояниями
        CheckStateTransitions(ref stateData);
    }

    public void ChangeState(NavMeshStateType newStateType, ref NavMeshStateData stateData)
    {
        if (_currentStateType == newStateType) return;

        _currentState?.Exit(ref stateData, _input);
        
        _currentState = newStateType switch
        {
            NavMeshStateType.Idle => _idleState,
            NavMeshStateType.Follow => _followState,
            NavMeshStateType.Attack => _attackState,
            _ => _idleState
        };
        
        _currentStateType = newStateType;
        _currentState.Enter(ref stateData, _input);
    }

    private void CheckStateTransitions(ref NavMeshStateData stateData)
    {
        if (stateData.TargetTransform == null)
        {
            if (_currentStateType != NavMeshStateType.Idle)
            {
                ChangeState(NavMeshStateType.Idle, ref stateData);
            }
            return;
        }

        var distanceToTarget = Vector3.Distance(stateData.Transform.position, stateData.TargetTransform.position);

        // Переход в атаку
        if (distanceToTarget <= stateData.Settings.AttackRange && 
            _currentStateType != NavMeshStateType.Attack)
        {
            ChangeState(NavMeshStateType.Attack, ref stateData);
            return;
        }

        // Переход к преследованию
        if (distanceToTarget > stateData.Settings.AttackRange * 1.2f && 
            _currentStateType == NavMeshStateType.Attack)
        {
            ChangeState(NavMeshStateType.Follow, ref stateData);
        }
    }
}