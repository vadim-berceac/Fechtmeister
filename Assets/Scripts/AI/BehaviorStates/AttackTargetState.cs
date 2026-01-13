using UnityEngine;

public struct AttackTargetState : INavMeshState
{
    private const float ATTACK_COOLDOWN = 1.5f; // Время между атаками
    
    public void Enter(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        input.InvokeMove(Vector2.zero);
        input.SetRunState(false);
        data.LastAttackTime = Time.time - ATTACK_COOLDOWN; // Позволяем атаковать сразу
    }

    public void Update(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        if (data.TargetTransform == null)
        {
            input.InvokeMove(Vector2.zero);
            return;
        }

        var distanceToTarget = Vector3.Distance(data.Transform.position, data.TargetTransform.position);
        
        // Позиционирование относительно цели
        HandlePositioning(ref data, input, distanceToTarget);
        
        // Вращение к цели
        NavMeshUtility.RotateTowardsTarget(
            data.Transform, 
            data.TargetTransform.position, 
            data.Settings.AttackRotationSpeed
        );
        
        // Вращение камеры к цели
        UpdateCameraLook(ref data, input);
        
        // Атака
        TryAttack(ref data, input, distanceToTarget);
    }

    public void Exit(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        input.InvokeMove(Vector2.zero);
        input.InvokeLook(Vector2.zero);
    }

    private static void HandlePositioning(ref NavMeshStateData data, NavMeshCharacterInput input, float distanceToTarget)
    {
        var settings = data.Settings;
        
        // Если слишком далеко - подойти ближе
        if (distanceToTarget > settings.AttackRange * 0.9f)
        {
            var direction = (data.TargetTransform.position - data.Transform.position).normalized;
            var localDirection = data.Transform.InverseTransformDirection(direction);
            var moveInput = new Vector2(localDirection.x, localDirection.z).normalized;
            input.InvokeMove(moveInput);
        }
        // Если слишком близко - отступить
        else if (distanceToTarget < settings.AttackRange * 0.5f)
        {
            var direction = (data.Transform.position - data.TargetTransform.position).normalized;
            var localDirection = data.Transform.InverseTransformDirection(direction);
            var moveInput = new Vector2(localDirection.x, localDirection.z).normalized * 0.5f;
            input.InvokeMove(moveInput);
        }
        // Оптимальная дистанция - стоять на месте
        else
        {
            input.InvokeMove(Vector2.zero);
        }
    }

    private static void UpdateCameraLook(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        // Вычисляем направление к цели для камеры
        var directionToTarget = data.TargetTransform.position - data.Transform.position;
        
        // Преобразуем в локальное пространство для камеры
        var localDirection = data.Transform.InverseTransformDirection(directionToTarget);
        
        // Вычисляем углы для look input (pitch и yaw)
        var horizontalAngle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        var verticalAngle = Mathf.Atan2(localDirection.y, new Vector2(localDirection.x, localDirection.z).magnitude) * Mathf.Rad2Deg;
        
        // Нормализуем углы для input системы (-1 до 1)
        var lookInput = new Vector2(
            Mathf.Clamp(horizontalAngle / 45f, -1f, 1f),
            Mathf.Clamp(verticalAngle / 45f, -1f, 1f)
        );
        
        input.InvokeLook(lookInput);
    }

    private static void TryAttack(ref NavMeshStateData data, NavMeshCharacterInput input, float distanceToTarget)
    {
        // Проверяем cooldown
        if (Time.time - data.LastAttackTime < ATTACK_COOLDOWN)
        {
            return;
        }

        // Проверяем дистанцию
        if (distanceToTarget > data.Settings.AttackRange)
        {
            return;
        }

        // Проверяем угол обзора (атакуем только если смотрим на цель)
        var directionToTarget = (data.TargetTransform.position - data.Transform.position).normalized;
        var dot = Vector3.Dot(data.Transform.forward, directionToTarget);
        
        if (dot < data.Settings.AttackAngleDotThreshold) // ~30 градусов
        {
            return;
        }

        // Выполняем атаку
        input.InvokeAttack();
        data.LastAttackTime = Time.time;
    }
}