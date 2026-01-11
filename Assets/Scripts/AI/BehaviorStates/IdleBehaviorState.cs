using UnityEngine;

public struct IdleBehaviorState : INavMeshState
{
    public void Enter(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        input.InvokeMove(Vector2.zero);
    }

    public void Update(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        // Поворачиваемся к цели если она есть
        if (data.TargetTransform != null)
        {
            NavMeshUtility.RotateTowardsTarget(
                data.Transform, 
                data.TargetTransform.position, 
                data.Settings.IdleRotationSpeed
            );
        }
    }

    public void Exit(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
    }
}