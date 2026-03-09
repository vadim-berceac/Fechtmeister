using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AttackTargetAction ", story: "Attack target", category: "Action/Combat",
    id: "19de88cb64fc20c7a6057050b50c32fc")]
public partial class AttackTargetAction : Action
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
    private float _attackRange;
    private RangeTypes _rangeType;
    private bool _isAiming;
    private bool _aimBlockActivated;
    private float _lastAttackTime = -999f;
    private float _aimStartTime;
    private bool _wasMeleeRange; // для отслеживания пересечения порога Mixed
    private const float CooldownBuffer = 0.1f;

    protected override Status OnStart()
    {
        if (_inputSystem == null)
            _inputSystem = GameObject.GetComponent<BehaviorNewInput>();

        if (_selfTransform == null)
            _selfTransform = GameObject.transform;

        _attackRange = this.GetAttackRange(CharacterCore.Value);
        _rangeType = this.GetRangeTypes(CharacterCore.Value);

        _isAiming = false;
        _aimBlockActivated = false;
        _aimStartTime = 0f;
        _wasMeleeRange = false;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_inputSystem == null || !_inputSystem.IsEnabled)
            return Status.Failure;

        var targetStatus = CheckTarget();
        if (targetStatus == Status.Success)
        {
            SetAttackingFlag(false);
            return Status.Success;
        }

        var targetTransform = CurrentTarget.Value.transform;
        var currentPos = _selfTransform.position;
        var targetPos = targetTransform.position;
        var distance = (currentPos - targetPos).magnitude;

        RotateToTarget(targetPos, currentPos);

        if (_rangeType == RangeTypes.Mixed)
            HandleMixedRangeTransitions(distance);

        if (_isAiming)
        {
            SetAttackingFlag(true);
            HandleAiming();
            _inputSystem.SimulateMove(Vector2.zero);
            return Status.Running;
        }

        SetAttackingFlag(false);
        HandlePositioning(distance);
        CheckAttackConditions(distance);

        return Status.Running;
    }

    private void HandleMixedRangeTransitions(float distance)
    {
        var isMeleeRange = distance <= MixedRangeMeleeRange.Value;

        // дальний → ближний: прерываем прицеливание
        if (!_wasMeleeRange && isMeleeRange)
        {
            StopAiming();
            _lastAttackTime = -999f;
        }
        // ближний → дальний: начинаем прицеливание немедленно
        else if (_wasMeleeRange && !isMeleeRange)
        {
            _lastAttackTime = -999f;
        }

        _wasMeleeRange = isMeleeRange;
    }

    private void SetAttackingFlag(bool isAttacking)
    {
        if (IsAttacking != null)
            IsAttacking.Value = isAttacking;
    }

    private void HandleAiming()
    {
        if (!_aimBlockActivated)
        {
            _inputSystem.SimulateBlock();
            _aimBlockActivated = true;
        }

        var aimDuration = Time.time - _aimStartTime;

        if (aimDuration < AimTime.Value)
            return;

        _inputSystem.SimulateAttack();
        _lastAttackTime = Time.time;
    }

    private Status CheckTarget()
    {
        if (CurrentTarget.Value == null || CurrentTarget.Value.IsDestroyed)
        {
            _inputSystem.SimulateMove(Vector2.zero);
            StopAiming();
            return Status.Success;
        }
        return Status.Failure;
    }

    private void RotateToTarget(Vector3 targetPos, Vector3 currentPos)
    {
        var direction = (targetPos - currentPos).normalized;
        var horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;

        if (horizontalDirection.magnitude <= 0.1f)
            return;

        var targetRotation = Quaternion.LookRotation(horizontalDirection);
        _selfTransform.rotation = Quaternion.Slerp(
            _selfTransform.rotation,
            targetRotation,
            RotationSpeed.Value * Time.deltaTime
        );
    }

    private void HandlePositioning(float distance)
    {
        if (_rangeType == RangeTypes.Mixed)
        {
            _inputSystem.SimulateMove(Vector2.zero);
            return;
        }

        if (distance > _attackRange * 0.9f)
            _inputSystem.SimulateMove(Vector2.up * 0.5f);
        else if (distance < _attackRange * 0.5f)
            _inputSystem.SimulateMove(Vector2.down * 0.5f);
        else
            _inputSystem.SimulateMove(Vector2.zero);
    }

    private void CheckAttackConditions(float distance)
    {
        if (_isAiming)
            return;

        var usesAiming = _rangeType == RangeTypes.Ranged ||
                         (_rangeType == RangeTypes.Mixed && distance > MixedRangeMeleeRange.Value);
        var effectiveCooldown = usesAiming
            ? AttackCooldown.Value + CooldownBuffer
            : AttackCooldown.Value;

        if (Time.time - _lastAttackTime < effectiveCooldown)
            return;

        switch (_rangeType)
        {
            case RangeTypes.Melee:
                BeginMeleeAttack();
                break;

            case RangeTypes.Ranged:
                BeginRangedAttack();
                break;

            case RangeTypes.Mixed:
                if (distance <= MixedRangeMeleeRange.Value)
                    BeginMeleeAttack();
                else
                    BeginRangedAttack();
                break;
        }
    }

    private void BeginMeleeAttack()
    {
        _inputSystem.SimulateAttack();
        _lastAttackTime = Time.time;
    }

    private void BeginRangedAttack()
    {
        _isAiming = true;
        _aimStartTime = Time.time;
    }

    private void StopAiming()
    {
        if (_aimBlockActivated)
        {
            _inputSystem.SimulateBlock();
            _aimBlockActivated = false;
        }
        _isAiming = false;
        _aimStartTime = 0f;
    }

    protected override void OnEnd()
    {
        if (_inputSystem != null)
        {
            _inputSystem.SimulateMove(Vector2.zero);
            StopAiming();
        }
        SetAttackingFlag(false);
    }
}