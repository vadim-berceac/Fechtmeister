using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "RunState", menuName = "States/RunState")]
public class RunState: State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.PlayablesAnimatorController.OnEnter(Clips[0], EnterTransitionDuration);
        character.PlayablesAnimatorController.SetAnimationParameter(Clips[0].ParameterName, 0);
    }
    
    protected override void CheckSwitch(CharacterCore character)
    {
        if (Mathf.Abs(character.CharacterInputHandler.InputX) == 0 &&
            Mathf.Abs(character.CharacterInputHandler.InputY) == 0)
        {
            character.SetState(character.StatesContainer.IdleState);
        }
        
        if (!character.CharacterInputHandler.IsRun)
        {
            character.SetState(character.StatesContainer.WalkState);
        }
        
        if (character.CharacterInputHandler.IsWeaponDraw)
        {
            character.SetState(character.StatesContainer.WeaponOnState);
        }
        
        if (character.CharacterInputHandler.IsJump)
        {
            character.SetState(character.StatesContainer.JumpState);
        }
        
        if (!character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.FallState);
        }
        
        if (character.CharacterInputHandler.IsInventoryOpen)
        {
            character.SetState(character.StatesContainer.InventoryState);
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        character.PlayablesAnimatorController.UpdateMoveBlend(character.CharacterInputHandler.InputX, character.CharacterInputHandler.InputY);
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.CurrentSpeed.StopUpdateLastHorizontalSpeed();
    }
}