using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "AimState", menuName = "States/AimState")]
public class AimState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.AimStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsAimBlock)
        {
            character.SetState(character.StatesContainer.CombatIdleState);
        }
        
        if (character.CharacterInputHandler.IsAttack)
        {
            character.SetState(character.StatesContainer.ReleaseState);
        }
    }
}
