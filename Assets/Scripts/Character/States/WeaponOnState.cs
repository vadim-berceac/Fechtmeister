using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponOnState", menuName = "States/WeaponOnState")]
public class WeaponOnState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.WeaponType, itemInstanceData.AnimationType);
        character.LocomotionSettings.Animator.StopPlayback();
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.WeaponOnStateName, EnterTransitionDuration);
        
        character.AttackCounter.SetValue(itemInstanceData.AttackCounterSettings.AttacksResetDelay, itemInstanceData.AttackCounterSettings.AttacksCount);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
        
        if (character.LocomotionSettings.Animator.GetFloat(AnimationParams.ActionCurve) <= 0)
        {
           character.Inventory.WeaponOn();
        }
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) <= 0)
        {
            character.SetState(character.StatesContainer.CombatIdleState);
        }
    }
}
