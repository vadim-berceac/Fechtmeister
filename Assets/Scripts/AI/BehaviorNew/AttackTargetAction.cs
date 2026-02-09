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
    [SerializeReference] public BlackboardVariable<float> AttackCooldown;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed;
    [SerializeReference] public BlackboardVariable<float> AimTime;
    [SerializeReference] public BlackboardVariable<bool> IsAttacking; 

    private BehaviorNewInput _inputSystem;
    private Transform _selfTransform;
    private float _attackRange;
    private bool _isRangedWeapon;
    private bool _isAiming;
    private bool _aimBlockActivated;
    private bool _shotFired;
    private int _releaseBlockFrame;
    private float _lastAttackTime = -999f;
    private float _aimStartTime;
    private const float CooldownBuffer = 0.1f;

    protected override Status OnStart()
    {
        if (_inputSystem == null)
            _inputSystem = GameObject.GetComponent<BehaviorNewInput>();
        
        if (_selfTransform == null)
            _selfTransform = GameObject.transform;
    
        _attackRange = this.GetAttackRange(CharacterCore.Value);
        _isRangedWeapon = this.IsWeaponRanged(CharacterCore.Value);
    
        _isAiming = false;
        _aimBlockActivated = false;
        _shotFired = false;
        _releaseBlockFrame = -1;
        _aimStartTime = 0f;

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
    
        if (!_isAiming && !_shotFired)
        {
            var followStatus = CheckFollowing(distance);
            if (followStatus == Status.Failure)
            {
                SetAttackingFlag(false);
                return Status.Failure;
            }
        }
    
        RotateToTarget(targetPos, currentPos);
    
        if (_isAiming || _shotFired)
        {
            SetAttackingFlag(true);
            HandleAiming();
            _inputSystem.SimulateMove(Vector2.zero);
            LevelTransform(_selfTransform);
            return Status.Running;
        }
    
        SetAttackingFlag(false);
        HandlePositioning(targetPos, currentPos, distance);
        CheckAttackConditions();
        LevelTransform(_selfTransform);
    
        return Status.Running;
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
        
        float aimDuration = Time.time - _aimStartTime;
        
        if (aimDuration >= AimTime.Value && !_shotFired)
        {
            _inputSystem.SimulateAttack();
            _shotFired = true;
            _releaseBlockFrame = Time.frameCount + 2;
            _lastAttackTime = Time.time;
            return;
        }
        
        if (_shotFired && Time.frameCount >= _releaseBlockFrame)
        {
            _inputSystem.SimulateBlock();
            _aimBlockActivated = false;
            _shotFired = false;
            _isAiming = false;
            _aimStartTime = 0f;
        }
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

    private Status CheckFollowing(float distance)
    {
        if (distance > _attackRange * 1.2f)
        {
            _inputSystem.SimulateMove(Vector2.zero);
            StopAiming();
            return Status.Failure;
        }
        return Status.Success;
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

    private void HandlePositioning(Vector3 targetPos, Vector3 currentPos, float distance)
    {
        if (distance > _attackRange * 0.9f)
        {
            _inputSystem.SimulateMove(Vector2.up * 0.5f);
        }
        else if (distance < _attackRange * 0.5f)
        {
            _inputSystem.SimulateMove(Vector2.down * 0.5f);
        }
        else
        {
            _inputSystem.SimulateMove(Vector2.zero);
        }
    }

    private void CheckAttackConditions()
    {
        if (_isAiming)
            return;
        
        float effectiveCooldown = _isRangedWeapon 
            ? AttackCooldown.Value + CooldownBuffer 
            : AttackCooldown.Value;
        
        if (Time.time - _lastAttackTime < effectiveCooldown)
            return;
        
        if (!_isRangedWeapon)
        {
            BeginMeleeAttack();
        }
        else 
        {
            BeginRangedAttack();
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
        _shotFired = false;
        _aimStartTime = 0f;
    }

    private void LevelTransform(Transform transform)
    {
        var euler = transform.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        transform.eulerAngles = euler;
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