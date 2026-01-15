using UnityEngine;

public struct IdleBehaviorState : INavMeshState
{
    public void Enter(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        input.InvokeMove(Vector2.zero);
        input.InvokeLook(Vector2.zero);
        input.SetRunState(false);
        
        // Запоминаем время входа в idle для задержки убирания оружия
        if (data.HasWeaponDrawn)
        {
            data.WeaponDrawTime = Time.time;
        }
    }

    public void Update(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        // Убираем оружие с задержкой
        if (data.HasWeaponDrawn && ShouldHolsterWeapon(ref data))
        {
            input.InvokeDrawWeapon();
            data.HasWeaponDrawn = false;
            Debug.Log("[NavMeshInput] Holstering weapon after idle delay");
        }
        
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
        // Сбрасываем время входа в idle
        data.WeaponDrawTime = 0f;
    }
    
    private static bool ShouldHolsterWeapon(ref NavMeshStateData data)
    {
        // Проверяем, прошла ли задержка с момента входа в idle
        var timeSinceIdle = Time.time - data.WeaponDrawTime;
        return timeSinceIdle >= data.Settings.WeaponDrawDelay;
    }
}