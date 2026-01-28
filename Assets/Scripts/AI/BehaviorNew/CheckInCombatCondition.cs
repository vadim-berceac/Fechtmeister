using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[Condition(
    name: "Is In Combat", 
    story: "[IsInCombat] is true",
    category: "Conditions/Combat", 
    id: "70bd3d677e97f733968fc684b1dfb4f8"
)]
public partial class CheckInCombatCondition : Condition
{
    [SerializeReference] public BlackboardVariable<bool> IsInCombat;

    public override bool IsTrue()
    {
        if (IsInCombat == null)
        {
            return false;
        }

        bool result = IsInCombat.Value;
        return result;
    }
}