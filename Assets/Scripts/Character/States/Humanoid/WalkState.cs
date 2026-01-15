using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "WalkState", menuName = "States/WalkState")]
public class WalkState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => (character.Health.IsHitReactionEnabled), "GetHitState"),
            new(character => (Mathf.Abs(character.CharacterInputHandler.InputX) == 0 &&
                              Mathf.Abs(character.CharacterInputHandler.InputY) == 0), "IdleState"),
            new(character => (character.CharacterInputHandler.IsRun && character.Health.CurrentHealthNormalized >= 0.5), "RunState"),
            new(character => (character.Inventory.WeaponSystem.CanDrawWeapon()), "WeaponOnState"),
            new(character => (character.CharacterInputHandler.IsJump), "JumpState"),
            new(character => (!character.Gravity.Grounded), "FallState"),
            new(character => (character.CharacterInputHandler.IsInventoryOpen), "InventoryState"),
        };
    }
}