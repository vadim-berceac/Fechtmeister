using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AttackTargetByBossAction ", story: "Attack Target By Boss Action", category: "Action/Combat",
    id: "4c959e3c76940df22d78c95e549d2613")]
public partial class AttackTargetByBossAction : Action
{
    [SerializeReference] public BlackboardVariable<HealthComponent> CurrentTarget;
    [SerializeReference] public BlackboardVariable<CharacterCore> CharacterCore;
    [SerializeReference] public BlackboardVariable<float> MixedRangeMeleeRange;
    [SerializeReference] public BlackboardVariable<float> AttackCooldown;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed;
    [SerializeReference] public BlackboardVariable<float> AimTime;
    [SerializeReference] public BlackboardVariable<bool> IsAttacking;

    private BehaviorNewInput _inputSystem;
    private Transform _selfTransform;
    private RangeTypes _rangeType;

    private bool _isAiming;
    private bool _shotFired;
    private float _aimStartTime;
    private float _lastAttackTime = -999f;

    private const float CooldownBuffer = 0.1f;

    protected override Status OnStart()
    {
        if (_inputSystem == null)
            _inputSystem = GameObject.GetComponent<BehaviorNewInput>();
        if (_selfTransform == null)
            _selfTransform = GameObject.transform;

        _rangeType = this.GetRangeTypes(CharacterCore.Value);
        _shotFired = false;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_inputSystem == null || !_inputSystem.IsEnabled)
            return Status.Failure;

        if (CurrentTarget.Value == null || CurrentTarget.Value.IsDestroyed)
        {
            Cleanup();
            return Status.Success;
        }

        float distance = (_selfTransform.position - CurrentTarget.Value.transform.position).magnitude;

        // inMeleeRange — единственный источник истины для Mixed, вычисляется из сырой дистанции
        bool inMeleeRange = _rangeType == RangeTypes.Melee ||
                            (_rangeType == RangeTypes.Mixed && distance <= MixedRangeMeleeRange.Value);

        RotateToTarget();
        _inputSystem.SimulateMove(Vector2.zero);

        // Выстрел был в прошлом фрейме — теперь снимаем блок
        if (_shotFired)
        {
            _shotFired = false;
            StopAiming();
            _lastAttackTime = Time.time;
        }

        // Фаза прицеливания — не прерываем, даём выстрелу произойти.
        if (_isAiming)
        {
            SetAttackingFlag(true);
            if (!_inputSystem.IsAimBlockActive)
                _inputSystem.SimulateBlock();
            if (Time.time - _aimStartTime >= AimTime.Value)
            {
                _inputSystem.SimulateAttack();
                _shotFired = true; // StopAiming в следующем фрейме
            }
            return Status.Running;
        }

        SetAttackingFlag(false);

        float cooldown = inMeleeRange
            ? AttackCooldown.Value
            : AttackCooldown.Value + CooldownBuffer;

        if (Time.time - _lastAttackTime < cooldown)
            return Status.Running;

        if (inMeleeRange)
        {
            _inputSystem.SimulateAttack();
            _lastAttackTime = Time.time;
        }
        else
        {
            StartAiming();
        }

        return Status.Running;
    }

    private void StartAiming()
    {
        if (_inputSystem.IsAimBlockActive)
            _inputSystem.SimulateBlock(); // OFF — сброс до чистого состояния
        _inputSystem.SimulateBlock();     // ON
        _isAiming = true;
        _aimStartTime = Time.time;
    }

    private void StopAiming()
    {
        if (_inputSystem.IsAimBlockActive)
            _inputSystem.SimulateBlock(); // OFF
        _isAiming = false;
        _aimStartTime = 0f;
    }

    private void RotateToTarget()
    {
        var dir = CurrentTarget.Value.transform.position - _selfTransform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        _selfTransform.rotation = Quaternion.Slerp(
            _selfTransform.rotation,
            Quaternion.LookRotation(dir.normalized),
            RotationSpeed.Value * Time.deltaTime);
    }

    private void SetAttackingFlag(bool value)
    {
        if (IsAttacking != null)
            IsAttacking.Value = value;
    }

    private void Cleanup()
    {
        SetAttackingFlag(false);
    }

    protected override void OnEnd() => Cleanup();
}