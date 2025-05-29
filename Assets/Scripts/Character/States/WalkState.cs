using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "WalkState", menuName = "States/WalkState")]
public class WalkState : State
{
    public override void EnterState(CharacterCore character)
    {
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.WalkStateName, enterTransitionDuration, animationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
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

        if (character.CharacterInputHandler.IsSprint)
        {
            character.SetState(character.StatesContainer.RunState);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        
    }
}