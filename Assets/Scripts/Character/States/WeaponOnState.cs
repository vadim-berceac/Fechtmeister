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
        //тест
        character.TestWeaponInstance.AttachToBone(character.TestWeaponInstance.ItemData.BoneData[character.TestWeaponInstance.ItemData.ActiveBoneIndex].BonesType);
    }
}
