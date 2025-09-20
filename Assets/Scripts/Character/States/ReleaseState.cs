using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "ReleaseState", menuName = "States/ReleaseState")]
public class ReleaseState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        character.GraphCore.PlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
        character.GraphCore.PlayablesAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.GraphCore.PlayablesAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.PlayablesAnimatorController.IsCurrentClipFinished())
        {
            character.Inventory.ProjectileSystem.Shot();
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
    }
}
