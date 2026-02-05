using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ClearCombatStateAction", story: "Clear Combat State Action",
    category: "Action/Combat", id: "bff275451aede3da8fec194c8b5d256c")]
public partial class ClearCombatStateAction : Action
{
    [SerializeReference] public BlackboardVariable<HealthComponent> CurrentTarget;
    [SerializeReference] public BlackboardVariable<bool> IsInCombat;
    [SerializeReference] public BlackboardVariable<bool> HasWeaponDrawn;
    [SerializeReference] public BlackboardVariable<bool> IsWeaponReady;
    [SerializeReference] public BlackboardVariable<bool> OnHitReaction;

    private BehaviorNewInput _inputSystem;
    private BehaviorGraphAgent _agent;  // ← добавляем ссылку на агент

    protected override Status OnStart()
    {
        Debug.Log("[ClearCombatState] OnStart called");

        // Получаем BehaviorGraphAgent один раз (лучше всего в OnCreate, но здесь тоже ок)
        if (_agent == null)
        {
            _agent = GameObject.GetComponent<BehaviorGraphAgent>();
            if (_agent == null)
            {
                Debug.LogError("[ClearCombatState] BehaviorGraphAgent not found on this GameObject!");
                return Status.Failure;
            }
        }

        if (_inputSystem == null)
            _inputSystem = GameObject.GetComponent<BehaviorNewInput>();

        _inputSystem.SimulateDrawWeapon(); 
        IsInCombat.Value = false;  
        HasWeaponDrawn.Value = false;
        IsWeaponReady.Value = false;
        OnHitReaction.Value = false;
        CurrentTarget.Value = null;

        Debug.Log($"<color=blue>[AI] Combat cleared, sheathing weapon, returning to patrol</color>");

        _agent.Restart();
        if (_agent.Graph != null)
        {
            _agent.Graph.Tick();  
        }

        return Status.Success;
    }
}