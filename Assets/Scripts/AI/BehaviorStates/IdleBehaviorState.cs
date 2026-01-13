using UnityEngine;

public struct IdleBehaviorState : INavMeshState
{
    public void Enter(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        input.InvokeMove(Vector2.zero);
        input.InvokeLook(Vector2.zero);
        input.SetRunState(false);
        
        // Убираем оружие при переходе в idle
        if (data.HasWeaponDrawn)
        {
            input.InvokeDrawWeapon();
            data.HasWeaponDrawn = false;
        }
    }

    public void Update(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        // Если есть цель - поворачиваемся к ней
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
        // Cleanup если нужен
    }
}