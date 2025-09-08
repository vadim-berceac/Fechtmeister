using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "JumpState", menuName = "States/JumpState")]
public class JumpState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.JumpStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
        character.MoveLocal(character.CharacterInputHandler.DirVector3 + character.CashedTransform.up, character.CurrentSpeed.LastNotNullHorizontalSpeed);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.Gravity.Grounded  && character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed)  <= 0.4)
        {
            character.SetState(character.StatesContainer.LandingState);
        }
        
        if (!character.Gravity.Grounded
            && character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) <= 0.5)
        {
            character.SetState(character.StatesContainer.FallState);
        }
    }
}
