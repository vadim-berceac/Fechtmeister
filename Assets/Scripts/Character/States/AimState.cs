using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "AimState", menuName = "States/AimState")]
public class AimState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
        character.CharacterPlayablesAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.CharacterPlayablesAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsAimBlock)
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
        
        if (character.CharacterInputHandler.IsAttack)
        {
            character.SetState(character.StatesContainer.GetState("ReleaseState"));
        }
    }
}
