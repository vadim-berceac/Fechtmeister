using System.Linq;
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
        var clipSet = Clips.FirstOrDefault(n => n.ParamValue == itemInstanceData.AnimationType);
        character.PlayablesAnimatorController.OnEnter(clipSet, EnterTransitionDuration);
        character.PlayablesAnimatorController.SetAnimationParameter(clipSet.ParameterName, character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.Inventory.ProjectileSystem.IsProjectileLoaded)
        {
            character.SetState(character.StatesContainer.GetState("ReloadProjectileState"));
        }
        
        if (character.Gravity.Grounded &&  character.PlayablesAnimatorController.IsBlendFinished())
        {
            character.SetState(character.StatesContainer.GetState("AimState"));
        }
        
        if (!character.CharacterInputHandler.IsAimBlock)
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
    }
}
