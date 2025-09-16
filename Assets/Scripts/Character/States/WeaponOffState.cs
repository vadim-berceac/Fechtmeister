using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "WeaponOffState", menuName = "States/WeaponOffState")]
public class WeaponOffState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        character.PlayablesAnimatorController.OnEnter(Clips[0], EnterTransitionDuration);
        character.PlayablesAnimatorController.SetAnimationParameter(Clips[0].ParameterName, itemInstanceData.AnimationType);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.PlayablesAnimatorController.IsBlendFinished())
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.PlayablesAnimatorController.IsActionEnabled)
        {
            character.Inventory.WeaponOff();
            character.PlayablesAnimatorController.ResetActionFlag();
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Inventory.ProjectileSystem.SetProjectileLoaded(false);
    }
}
