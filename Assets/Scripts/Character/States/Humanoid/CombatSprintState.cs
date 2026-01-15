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
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
            new(character => !character.CharacterInputHandler.IsRun, "CombatRunState"),
            new(character => character.CharacterInputHandler.TargetInputMagnitude < 0.2f, "SprintStopState"),
            new(character => character.Inventory.WeaponSystem.CanUnDrawWeapon(), "WeaponOffState"),
            new(character => character.CharacterInputHandler.IsJump, "JumpState"),
            new(character => !character.Gravity.Grounded, "FallState"),
            new (c => c.CharacterInputHandler.IsAimBlock && c.Inventory.WeaponSystem.WeaponInstanceIsRanged 
                                                         && c.Inventory.ProjectileSystem.HasProjectiles(), "LoadState"),
            new(character => character.CharacterInputHandler.IsJump, "JumpState"),
        };
    }
}
