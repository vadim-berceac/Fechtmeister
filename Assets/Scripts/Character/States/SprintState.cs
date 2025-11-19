using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SprintState", menuName = "States/SprintState")]
public class SprintState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => (!character.CharacterInputHandler.IsRun), "RunState"),
            new(character => (character.CharacterInputHandler.TargetInputMagnitude < 0.2f), "SprintStopState"),
            new(character => (character.CharacterInputHandler.IsWeaponDraw), "WeaponOnState"),
            new(character => (character.CharacterInputHandler.IsJump), "JumpState"),
            new(character => (!character.Gravity.Grounded), "FallState"),
            new(character => (character.CharacterInputHandler.IsInventoryOpen), "InventoryState"),
            new(character => (character.Health.IsHitReactionEnabled), "GetHitState"),
            new(character => (character.Health.IsDestroyed), "DeathState"),
        };
    }
}