using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SprintState", menuName = "States/SprintState")]
public class SprintState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => (character.Health.IsHitReactionEnabled), "GetHitState"),
            new(character => (character.CharacterInputHandler.TargetInputMagnitude < 0.2f 
                              || !character.CharacterInputHandler.IsRun), "SprintStopState"),
            new(character => (character.Inventory.IsWeaponOn), "CombatSprintState"),
            new(character => (character.CharacterInputHandler.IsJump), "JumpState"),
            new(character => (!character.Gravity.Grounded), "FallState"),
            new(character => (character.CharacterInputHandler.IsInventoryOpen), "InventoryState"),
        };
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.Inventory.WeaponSystem.CanDrawWeapon() 
            && character.GraphCore.UpperBodyLayerController.IsComplete())
        {
            character.SetSubState(character.StatesSet.GetState("WeaponOnSubState"));
        } 
    }
}