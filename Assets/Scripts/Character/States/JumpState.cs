using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "JumpState", menuName = "States/JumpState")]
public class JumpState : State
{
    public override void EnterState(CharacterCore character)
    {
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.JumpStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.Grounded && !character.CharacterInputHandler.IsWeaponDraw 
                               && character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) <= 0)
        {
            character.SetState(character.StatesContainer.IdleState);
        }
    }

    public override void ExitState(CharacterCore character)
    {
       
    }
}
