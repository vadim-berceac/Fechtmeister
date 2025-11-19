using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatSprintState", menuName = "States/CombatSprintState")]
public class CombatSprintState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => !character.CharacterInputHandler.IsRun, "CombatRunState"),
            new(character => character.CharacterInputHandler.TargetInputMagnitude < 0.2f, "SprintStopState"),
            new(character => !character.CharacterInputHandler.IsWeaponDraw && !character.GraphCore.FullBodyAnimatorController.IsTransitioning, "WeaponOffState"),
            new(character => character.CharacterInputHandler.IsJump, "JumpState"),
            new(character => !character.Gravity.Grounded, "FallState"),
            new(character => character.CharacterInputHandler.IsAimBlock && character.Inventory.WeaponSystem.WeaponInstanceIsRanged, "LoadState"),
            new(character => character.CharacterInputHandler.IsJump, "JumpState"),
            new(character => character.Health.IsDestroyed, "DeathState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
        };
    }
}
