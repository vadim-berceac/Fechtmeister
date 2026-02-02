using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[Condition(
    name: "Has Target", 
    story: "[CurrentTarget] exists [WeaponReady]",  
    category: "Conditions/Combat", 
    id: "9869341555042a31c94556a5656c1fc6"
)]
public partial class CheckTargetExistsCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> CurrentTarget;
    [SerializeReference] public BlackboardVariable<bool> WeaponReady;

    public override bool IsTrue()
    {
        if (CurrentTarget == null)
        {
            Debug.LogWarning("[CheckTarget] CurrentTarget variable is not linked!");
            return false;
        }

        bool hasTarget = CurrentTarget.Value != null && WeaponReady;
        
        return hasTarget;
    }
}