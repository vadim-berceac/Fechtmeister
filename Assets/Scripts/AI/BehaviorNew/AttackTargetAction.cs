using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AttackTargetAction ", story: "Attack target", category: "Action/Combat", id: "19de88cb64fc20c7a6057050b50c32fc")]
public partial class AttackTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<HealthComponent> CurrentTarget;
    [SerializeReference] public BlackboardVariable<CharacterCore> CharacterCore;
    [SerializeReference] public BlackboardVariable<float> AttackCooldown;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed;
    
    private BehaviorNewInput _inputSystem;
    private Transform _selfTransform;
    private float _attackRange;
    private bool _isRangedWeapon;
    private float _lastAttackTime = -999f;

    protected override Status OnStart()
    {
        if (_inputSystem == null)
            _inputSystem = GameObject.GetComponent<BehaviorNewInput>();
        
        if (_selfTransform == null)
            _selfTransform = GameObject.transform;
        
        _attackRange = this.GetAttackRange(CharacterCore.Value);
        _isRangedWeapon = this.IsWeaponRanged(CharacterCore.Value);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_inputSystem == null || !_inputSystem.IsEnabled)
            return Status.Failure;

        // Проверяем наличие цели
        if (CurrentTarget.Value == null || CurrentTarget.Value.IsDestroyed)
        {
            _inputSystem.SimulateMove(Vector2.zero);
            return Status.Success; // Цель исчезла, выходим из состояния боя
        }

        var targetTransform = CurrentTarget.Value.transform;
        var currentPos = _selfTransform.position;
        var targetPos = targetTransform.position;
        
        var distance = (currentPos - targetPos).magnitude;
        
        // Если цель слишком далеко - возвращаемся к преследованию
        if (distance > _attackRange * 1.2f)
        {
            _inputSystem.SimulateMove(Vector2.zero);
            return Status.Failure;
        }

        // Поворачиваемся к цели
        var direction = (targetPos - currentPos).normalized;
        var horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;
        
        if (horizontalDirection.magnitude > 0.1f)
        {
            var targetRotation = Quaternion.LookRotation(horizontalDirection);
            _selfTransform.rotation = Quaternion.Slerp(
                _selfTransform.rotation, 
                targetRotation, 
                RotationSpeed.Value * Time.deltaTime
            );
        }

        // Позиционирование (подходим/отступаем)
        HandlePositioning(distance);

        // Атакуем с учётом cooldown
        if (Time.time - _lastAttackTime >= AttackCooldown.Value)
        {
            _inputSystem.SimulateAttack();
            _lastAttackTime = Time.time;
        }
        
        // Выравниваем капсулу
        var euler = _selfTransform.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        _selfTransform.eulerAngles = euler;
        
        return Status.Running;
    }

    private void HandlePositioning(float distance)
    {
        // Если слишком далеко - подойти
        if (distance > _attackRange * 0.9f)
        {
            _inputSystem.SimulateMove(Vector2.up * 0.5f);
        }
        // Если слишком близко - отступить
        else if (distance < _attackRange * 0.5f)
        {
            _inputSystem.SimulateMove(Vector2.down * 0.5f);
        }
        // Оптимальная дистанция - стоим
        else
        {
            _inputSystem.SimulateMove(Vector2.zero);
        }
    }

    protected override void OnEnd()
    {
        if (_inputSystem != null)
            _inputSystem.SimulateMove(Vector2.zero);
    }
}

