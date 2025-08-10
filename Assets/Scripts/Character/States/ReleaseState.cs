using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "ReleaseState", menuName = "States/ReleaseState")]
public class ReleaseState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.VerticalAngleToTarget, character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.ReleaseStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) == 0)
        {
            Debug.Log(character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed));
            character.SetState(character.StatesContainer.CombatIdleState);
        }
    }
}
