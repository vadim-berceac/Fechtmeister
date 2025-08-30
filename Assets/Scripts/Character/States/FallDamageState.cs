using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "FallDamageState", menuName = "States/FallDamageState")]
public class FallDamageState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.FallDamageStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
       
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Health.EnableHitReaction(false);
    }
}
