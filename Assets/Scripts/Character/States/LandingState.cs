using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "LandingState", menuName = "States/LandingState")]
public class LandingState: State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.LandingStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
         if (!character.CharacterInputHandler.IsWeaponDraw && character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) <= 0)
         {
             character.SetState(character.StatesContainer.IdleState);
         }
        
         if (character.CharacterInputHandler.IsWeaponDraw && character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) <= 0)
         {
             character.SetState(character.StatesContainer.CombatIdleState);
         }
    }
}