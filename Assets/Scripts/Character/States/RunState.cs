using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "RunState", menuName = "States/RunState")]
public class RunState: State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.RunStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
        
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.InputX,character.CharacterInputHandler.InputX);
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.InputY,character.CharacterInputHandler.InputY);
    }

    public override void CheckSwitch(CharacterCore character)
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
            character.CurrentSpeed.StopUpdateLastHorizontalSpeed();
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
}