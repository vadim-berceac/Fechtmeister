using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponOnState", menuName = "States/WeaponOnState")]
public class WeaponOnState : State
{
    public override void EnterState(CharacterCore character)
    {
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.WeaponType, character.TempWeaponData.AnimationType);
        character.LocomotionSettings.Animator.StopPlayback();
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.WeaponOnStateName, EnterTransitionDuration);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
        
        if (character.LocomotionSettings.Animator.GetFloat(AnimationParams.ActionCurve) <= 0)
        {
            character.WeaponSystem.InstanceInHands.AttachToBone(character.WeaponSystem.InstanceInHands.Instance, 
                character.WeaponSystem.InstanceInHands.ItemData.BoneData[0]);
        }
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) <= 0)
        {
            character.SetState(character.StatesContainer.CombatIdleState);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        
    }
}
