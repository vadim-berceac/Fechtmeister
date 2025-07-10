using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatRunState", menuName = "States/CombatRunState")]
public class CombatRunState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.RunStateName, EnterTransitionDuration, AnimationLayer);
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
        
        if (!character.CharacterInputHandler.IsSprint)
        {
            character.SetState(character.StatesContainer.CombatWalkState);
        }
        
        if (!character.CharacterInputHandler.IsWeaponDraw)
        {
            character.SetState(character.StatesContainer.WeaponOffState);
        }
    }
}
