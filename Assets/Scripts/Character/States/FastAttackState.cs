using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "FastAttackState", menuName = "States/FastAttackState")]
public class FastAttackState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.AttackCounter, character.AttackCounter.GetValue());
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.Attack0StateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
        
        LegOverride(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.Gravity.Grounded &&  character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) == 0)
        {
            character.SetState(character.StatesContainer.CombatIdleState);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.LocomotionSettings.Animator.SetLayerWeight(3, 0);
    }

    private static void LegOverride(CharacterCore character)
    {
        if (character.Gravity.Grounded  && (Mathf.Abs(character.CharacterInputHandler.InputX) > 0 ||
                                            Mathf.Abs(character.CharacterInputHandler.InputY) > 0))
        {
            character.LocomotionSettings.Animator.SetLayerWeight(3, 1);
            return;
        }
        character.LocomotionSettings.Animator.SetLayerWeight(3, 0);
    }
}
