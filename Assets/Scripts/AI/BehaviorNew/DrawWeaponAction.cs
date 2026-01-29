using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Draw Weapon", 
    story: "Draw weapon and wait [WeaponDrawDelay] seconds, if not [OnHitReaction]",
    category: "Action/Combat", 
    id: "f7g8h9i0j1k2l3m4n5o6p7q8r9s0t1u2"
)]
public partial class DrawWeaponAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> HasWeaponDrawn;
    [SerializeReference] public BlackboardVariable<bool> IsWeaponReady;
    [SerializeReference] public BlackboardVariable<bool> OnHitReaction;
    [SerializeReference] public BlackboardVariable<float> WeaponDrawDelay;
    
    private BehaviorNewInput _inputSystem;
    private float _weaponDrawTime;
    private bool _isDrawing;
    private bool _waitingForHitReaction; // Ждем окончания хитреакции

    protected override Status OnStart()
    {
        Debug.Log("[DrawWeapon] OnStart called");
        
        if (_inputSystem == null)
            _inputSystem = GameObject.GetComponent<BehaviorNewInput>();

        if (HasWeaponDrawn == null || IsWeaponReady == null || WeaponDrawDelay == null || OnHitReaction == null)
        {
            Debug.LogError("[DrawWeapon] Blackboard variables are null");
            return Status.Failure;
        }

        // Если оружие УЖЕ вытащено И готово - сразу Success
        if (HasWeaponDrawn.Value && IsWeaponReady.Value)
        {
            Debug.Log("[DrawWeapon] Weapon already drawn and ready");
            _isDrawing = false;
            _waitingForHitReaction = false;
            return Status.Success;
        }

        // Если уже в процессе вытаскивания (повторный вход в OnStart)
        if (_isDrawing && HasWeaponDrawn.Value)
        {
            Debug.Log("[DrawWeapon] Already drawing, continuing...");
            return Status.Running;
        }

        // Если идет хитреакция - ждем её завершения
        if (OnHitReaction.Value)
        {
            Debug.Log("[DrawWeapon] Hit reaction active, waiting...");
            _waitingForHitReaction = true;
            _isDrawing = false;
            return Status.Running;
        }

        // Начинаем новое вытаскивание
        StartDrawing();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // Проверяем на случай если переменные изменились извне
        if (IsWeaponReady == null || WeaponDrawDelay == null || OnHitReaction == null)
            return Status.Failure;

        // Если ждем окончания хитреакции
        if (_waitingForHitReaction)
        {
            if (OnHitReaction.Value)
            {
                // Все еще идет хитреакция
                return Status.Running;
            }
            else
            {
                // Хитреакция закончилась, начинаем вытаскивание
                Debug.Log("[DrawWeapon] Hit reaction ended, starting draw");
                _waitingForHitReaction = false;
                StartDrawing();
                return Status.Running;
            }
        }

        // Если во время вытаскивания началась хитреакция - прерываемся
        if (_isDrawing && OnHitReaction.Value)
        {
            Debug.Log("[DrawWeapon] Hit reaction interrupted drawing, resetting");
            _isDrawing = false;
            _waitingForHitReaction = true;
            HasWeaponDrawn.Value = false;
            IsWeaponReady.Value = false;
            return Status.Running;
        }

        // Если оружие готово - завершаем
        if (IsWeaponReady.Value)
        {
            Debug.Log("[DrawWeapon] Weapon ready (set externally)");
            _isDrawing = false;
            return Status.Success;
        }

        // Проверяем таймер вытаскивания
        float timeSinceDrawn = Time.time - _weaponDrawTime;
        
        if (timeSinceDrawn >= WeaponDrawDelay.Value)
        {
            IsWeaponReady.Value = true;
            _isDrawing = false;
            Debug.Log($"[DrawWeapon] Weapon ready after {timeSinceDrawn:F2}s");
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        // Сбрасываем флаги только если оружие готово
        if (IsWeaponReady != null && IsWeaponReady.Value)
        {
            _isDrawing = false;
            _waitingForHitReaction = false;
        }
        
        Debug.Log($"[DrawWeapon] OnEnd called, isDrawing={_isDrawing}, waiting={_waitingForHitReaction}");
    }

    private void StartDrawing()
    {
        _inputSystem.SimulateDrawWeapon();
        HasWeaponDrawn.Value = true;
        IsWeaponReady.Value = false;
        _weaponDrawTime = Time.time;
        _isDrawing = true;
        _waitingForHitReaction = false;
        
        Debug.Log($"[DrawWeapon] Draw command sent, waiting {WeaponDrawDelay.Value}s for animation");
    }
}