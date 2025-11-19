using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatIdleState", menuName = "States/CombatIdleState")]
public class CombatIdleState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new (c => !c.CharacterInputHandler.IsWeaponDraw && !c.GraphCore.FullBodyAnimatorController.IsTransitioning, "WeaponOffState"),
            new (c => Mathf.Abs(c.CharacterInputHandler.InputX) > 0 && !c.GraphCore.FullBodyAnimatorController.IsTransitioning ||
                      Mathf.Abs(c.CharacterInputHandler.InputY) > 0 && !c.GraphCore.FullBodyAnimatorController.IsTransitioning, "CombatWalkState"),
            new (c => c.CharacterInputHandler.IsAttack && !c.Inventory.WeaponSystem.WeaponInstanceIsRanged 
                                                               && c.GraphCore.UpperBodyLayerController.IsComplete(), "FastAttackState"),
            new (c => c.CharacterInputHandler.IsAimBlock && c.Inventory.WeaponSystem.WeaponInstanceIsRanged, "LoadState"),
            new (c => c.CharacterInputHandler.IsJump, "JumpState"),
            new (c => !c.Gravity.Grounded, "FallState"),
            new (c => c.Health.IsHitReactionEnabled, "GetHitState"),
            new (c => c.Health.IsDestroyed, "DeathState"),
        };
    }
}
