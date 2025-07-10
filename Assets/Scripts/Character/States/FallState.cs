using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "FallState", menuName = "States/FallState")]
public class FallState :  State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.FallStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.IdleState);
        }
    }
}
