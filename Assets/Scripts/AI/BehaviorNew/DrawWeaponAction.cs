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
        if (_inputSystem == null)
            _inputSystem = GameObject.GetComponent<BehaviorNewInput>();

        if (HasWeaponDrawn == null || IsWeaponReady == null || WeaponDrawDelay == null || OnHitReaction == null)
        {
            return Status.Failure;
        }

        // Если оружие УЖЕ вытащено И готово - сразу Success
        if (HasWeaponDrawn.Value && IsWeaponReady.Value)
        {
            _isDrawing = false;
            _waitingForHitReaction = false;
            return Status.Success;
        }

        // Если уже в процессе вытаскивания (повторный вход в OnStart)
        if (_isDrawing && HasWeaponDrawn.Value)
        {
            return Status.Running;
        }

        // Если идет хитреакция - ждем её завершения
        if (OnHitReaction.Value)
        {
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
                _waitingForHitReaction = false;
                StartDrawing();
                return Status.Running;
            }
        }

        // Если во время вытаскивания началась хитреакция - прерываемся
        if (_isDrawing && OnHitReaction.Value)
        {
            _isDrawing = false;
            _waitingForHitReaction = true;
            HasWeaponDrawn.Value = false;
            IsWeaponReady.Value = false;
            return Status.Running;
        }

        // Если оружие готово - завершаем
        if (IsWeaponReady.Value)
        {
            _isDrawing = false;
            return Status.Success;
        }

        // Проверяем таймер вытаскивания
        float timeSinceDrawn = Time.time - _weaponDrawTime;
        
        if (timeSinceDrawn >= WeaponDrawDelay.Value)
        {
            IsWeaponReady.Value = true;
            _isDrawing = false;
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
    }

    private void StartDrawing()
    {
        _inputSystem.SimulateDrawWeapon();
        HasWeaponDrawn.Value = true;
        IsWeaponReady.Value = false;
        _weaponDrawTime = Time.time;
        _isDrawing = true;
        _waitingForHitReaction = false;
    }
}