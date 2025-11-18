using UnityEngine;

[CreateAssetMenu(fileName = "SprintState", menuName = "States/SprintState")]
public class SprintState : MovementState
{
    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsRun)
        {
            character.SetState(character.StatesContainer.GetState("RunState"));
        }
        
        if (character.CharacterInputHandler.TargetInputMagnitude < 0.2f)
        {
            character.SetState(character.StatesContainer.GetState("SprintStopState"));
        }
        
        if (character.CharacterInputHandler.IsWeaponDraw)
        {
            character.SetState(character.StatesContainer.GetState("WeaponOnState"));
        }
        
        if (character.CharacterInputHandler.IsJump)
        {
            character.SetState(character.StatesContainer.GetState("JumpState"));
        }
        
        if (!character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.GetState("FallState"));
        }
        
        if (character.CharacterInputHandler.IsInventoryOpen)
        {
            character.SetState(character.StatesContainer.GetState("InventoryState"));
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
        
        if (character.Health.IsDestroyed)
        {
            character.SetState(character.StatesContainer.GetState("DeathState"));
        }
    }
}