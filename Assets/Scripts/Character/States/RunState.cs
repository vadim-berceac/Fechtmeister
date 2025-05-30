using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "RunState", menuName = "States/RunState")]
public class RunState: State
{
    public override void EnterState(CharacterCore character)
    {
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
        
        if (!character.CharacterInputHandler.IsSprint)
        {
            character.SetState(character.StatesContainer.WalkState);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        
    }
}