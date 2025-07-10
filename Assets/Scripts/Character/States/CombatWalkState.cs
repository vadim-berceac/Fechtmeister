using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatWalkState", menuName = "States/CombatWalkState")]
public class CombatWalkState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.WalkStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
        
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.InputX,character.CharacterInputHandler.InputX);
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.InputY,character.CharacterInputHandler.InputY);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (Mathf.Abs(character.CharacterInputHandler.InputX) == 0 &&
            Mathf.Abs(character.CharacterInputHandler.InputY) == 0)
        {
            character.SetState(character.StatesContainer.CombatIdleState);
        }

        if (character.CharacterInputHandler.IsRun)
        {
            character.SetState(character.StatesContainer.CombatRunState);
        }

        if (!character.CharacterInputHandler.IsWeaponDraw)
        {
            character.SetState(character.StatesContainer.WeaponOffState);
        }
    }
}
