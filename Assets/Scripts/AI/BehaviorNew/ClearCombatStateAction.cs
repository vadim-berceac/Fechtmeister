using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ClearCombatStateAction ", story: "Clear Combat State Action",
    category: "Action/Combat", id: "bff275451aede3da8fec194c8b5d256c")]
public partial class ClearCombatStateAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> IsInCombat;
    [SerializeReference] public BlackboardVariable<bool> HasWeaponDrawn;
    [SerializeReference] public BlackboardVariable<bool> IsWeaponReady;

    private BehaviorNewInput _inputSystem;

    protected override Status OnStart()
    {
        Debug.Log("[DrawWeapon] OnStart called");
        
        if (_inputSystem == null)
            _inputSystem = GameObject.GetComponent<BehaviorNewInput>();
       
        IsInCombat.Value = false;
        _inputSystem.SimulateDrawWeapon();
        HasWeaponDrawn.Value = false;
        IsWeaponReady.Value = false;
        
        Debug.Log($"<color=blue>[AI] Combat cleared, sheathing weapon, returning to patrol</color>");
        
        return Status.Success;
    }
}