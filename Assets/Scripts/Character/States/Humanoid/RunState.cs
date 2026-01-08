using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RunState", menuName = "States/RunState")]
public class RunState: MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => !character.CharacterInputHandler.IsRun, "WalkState"),
            new(character => character.Inventory.WeaponSystem.CanDrawWeapon(), "WeaponOnState"),
            new(character => character.CharacterInputHandler.IsJump, "JumpState"),
            new(character => !character.Gravity.Grounded, "FallState"),
            new(character => character.CharacterInputHandler.IsInventoryOpen, "InventoryState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
            new(character => character.Gravity.Grounded && character.StateTimer.GetCurrentTimeInState() > 5f, "SprintState"),
        };
    }
}