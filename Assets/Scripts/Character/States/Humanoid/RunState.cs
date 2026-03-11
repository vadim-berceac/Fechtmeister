using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RunState", menuName = "States/RunState")]
public class RunState: MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new (c => c.Health.IsDestroyed, "DeathState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
            new(character => !character.CharacterInputHandler.IsRun, "WalkState"),
            new(character => (character.Inventory.IsWeaponOn), "CombatRunState"),
            new(character => character.CharacterInputHandler.IsJump, "JumpState"),
            new(character => !character.Gravity.Grounded, "FallState"),
            new(character => character.CharacterInputHandler.IsInventoryOpen, "InventoryState"),
            new(character => character.Gravity.Grounded && character.StateTimer.GetCurrentTimeInState() > 5f, "SprintState"),
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