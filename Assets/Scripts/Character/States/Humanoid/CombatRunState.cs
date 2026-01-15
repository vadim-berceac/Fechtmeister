using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatRunState", menuName = "States/CombatRunState")]
public class CombatRunState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
            new(character => !character.CharacterInputHandler.IsRun, "CombatWalkState"),
            new(character => character.Inventory.WeaponSystem.CanUnDrawWeapon(), "WeaponOffState"),
            new(character => character.CharacterInputHandler.IsAimBlock && character.Inventory.WeaponSystem.WeaponInstanceIsRanged, "LoadState"),
            new(character => character.CharacterInputHandler.IsJump, "JumpState"),
            new(character => character.Gravity.Grounded && character.StateTimer.GetCurrentTimeInState() > 5f, "CombatSprintState"),
            new(character => !character.Gravity.Grounded, "FallState"),
        };
    }
}
