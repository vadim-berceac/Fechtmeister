using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Draw Weapon", 
    story: "Draw weapon",
    category: "Action/Combat", 
    id: "f7g8h9i0j1k2l3m4n5o6p7q8r9s0t1u2"
)]
public partial class DrawWeaponAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> HasWeaponDrawn;
    
    private BehaviorNewInput _inputSystem;

    protected override Status OnStart()
    {
        if (_inputSystem == null)
            _inputSystem = GameObject.GetComponent<BehaviorNewInput>();

        if (HasWeaponDrawn == null || _inputSystem == null || !_inputSystem.IsEnabled)
            return Status.Failure;

        if (HasWeaponDrawn.Value)
            return Status.Success;

        _inputSystem.SimulateDrawWeapon();
        HasWeaponDrawn.Value = true;
        
        return Status.Success; 
    }
}