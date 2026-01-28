using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ClearCombatStateAction ", story: "Clear Combat State Action", category: "Action/Combat", id: "bff275451aede3da8fec194c8b5d256c")]
public partial class ClearCombatStateAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> CurrentTarget;
    [SerializeReference] public BlackboardVariable<bool> IsInCombat;
    [SerializeReference] public BlackboardVariable<bool> HasWeaponDrawn;

    protected override Status OnStart()
    {
        CurrentTarget.Value = null;
        IsInCombat.Value = false;
        HasWeaponDrawn.Value = false; 
        
        Debug.Log("[AI] Combat cleared, sheathing weapon, returning to patrol");
        
        return Status.Success;
    }
}