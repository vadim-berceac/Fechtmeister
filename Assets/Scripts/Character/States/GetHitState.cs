using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "GetHitState", menuName = "States/GetHitState")]
public class GetHitState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.AttackCounter, character.AttackCounter.GetValue());
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.GetHitStateName, EnterTransitionDuration, AnimationLayer);
        
        character.Health.EnableHitReaction(false);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.CharacterInputHandler.IsWeaponDraw &&  character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) == 0)
        {
            character.SetState(character.StatesContainer.CombatIdleState);
        }
        
        if (!character.CharacterInputHandler.IsWeaponDraw &&  character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) == 0)
        {
            character.SetState(character.StatesContainer.IdleState);
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetHitState);
        }
    }
}
