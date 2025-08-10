using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "LoadState", menuName = "States/LoadState")]
public class LoadState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.VerticalAngleToTarget, character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.LoadStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.Gravity.Grounded &&  character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) == 0)
        {
            character.SetState(character.StatesContainer.AimState);
        }
        
        if (!character.CharacterInputHandler.IsAimBlock)
        {
            character.SetState(character.StatesContainer.CombatIdleState);
        }
    }
}
