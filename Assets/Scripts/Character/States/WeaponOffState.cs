using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponOffState", menuName = "States/WeaponOffState")]
public class WeaponOffState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.WeaponType, 
            ((WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData).AnimationType);
        character.LocomotionSettings.Animator.StopPlayback();
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.WeaponOffStateName, EnterTransitionDuration);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
        
        if (character.LocomotionSettings.Animator.GetFloat(AnimationParams.ActionCurve) <= 0)
        {
           character.Inventory.WeaponOff();
        }
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) <= 0)
        {
            character.SetState(character.StatesContainer.IdleState);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.WeaponType, 0);
    }
}
