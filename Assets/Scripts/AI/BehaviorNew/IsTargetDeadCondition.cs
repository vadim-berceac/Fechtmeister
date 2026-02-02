using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable, GeneratePropertyBag]
[Condition(name: "Is Target Dead", story: "[Target] is dead", category: "Combat")]
public partial class IsTargetDeadCondition : Condition
{
    [SerializeReference] public BlackboardVariable<HealthComponent> Target;

    public override bool IsTrue()
    {
        Debug.Log("Checking IsTargetDeadCondition");
        var health = Target.Value;

        if (health == null)
        {
            Debug.Log("[IsTargetDead] Target is null in blackboard → treating as dead");
            return true;
        }

        bool isDead = health.IsDestroyed;
        Debug.Log($"[IsTargetDead] Target: {health.name}, IsDestroyed = {isDead}, CurrentHealth = {health.CurrentHealth}");

        if (isDead)
        {
            Debug.Log("[IsTargetDead] → Clearing Target reference");
            Target.Value = null;
        }

        return isDead;
    }
}