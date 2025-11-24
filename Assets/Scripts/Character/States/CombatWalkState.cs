using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatWalkState", menuName = "States/CombatWalkState")]
public class CombatWalkState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => Mathf.Abs(character.CharacterInputHandler.InputX) == 0 &&
                             Mathf.Abs(character.CharacterInputHandler.InputY) == 0, "CombatIdleState"),
            new(character => character.CharacterInputHandler.IsRun && character.Health.CurrentHealthNormalized >= 0.5, "CombatRunState"),
            new(character => !character.CharacterInputHandler.IsWeaponDraw, "WeaponOffState"),
            new(character => character.CharacterInputHandler.IsAimBlock && character.Inventory.WeaponSystem.WeaponInstanceIsRanged, "LoadState"),
            new(character => character.CharacterInputHandler.IsJump, "JumpState"),
            new(character => !character.Gravity.Grounded, "FallState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
        };
    }
}
