using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "LoadState", menuName = "States/LoadState")]
public class LoadState : State
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
        Debug.Log(character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
        character.GraphCore.PlayablesAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.Inventory.ProjectileSystem.IsProjectileLoaded)
        {
            character.SetState(character.StatesContainer.GetState("ReloadProjectileState"));
        }
        
        if (character.Gravity.Grounded && character.GraphCore.PlayablesAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("AimState"));
        }
        
        if (!character.CharacterInputHandler.IsAimBlock)
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
    }
}
