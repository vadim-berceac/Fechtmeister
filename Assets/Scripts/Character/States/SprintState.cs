using UnityEngine;

[CreateAssetMenu(fileName = "SprintState", menuName = "States/SprintState")]
public class SprintState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.PlayablesAnimatorController.SetAnimationState(this, 0);
    }
    
    protected override void CheckSwitch(CharacterCore character)
    {
        if (Mathf.Abs(character.CharacterInputHandler.InputX) == 0 &&
            Mathf.Abs(character.CharacterInputHandler.InputY) == 0)
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
        
        if (character.CharacterInputHandler.InputY < 0)
        {
            character.SetState(character.StatesContainer.GetState("RunState"));
        }
        
        if (!character.CharacterInputHandler.IsRun)
        {
            character.SetState(character.StatesContainer.GetState("WalkState"));
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
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.GraphCore.PlayablesAnimatorController.Move(character.CharacterInputHandler.InputX, character.CharacterInputHandler.InputY);
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.CurrentSpeed.StopUpdateLastHorizontalSpeed();
    }
}
