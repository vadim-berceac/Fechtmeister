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
        character.GraphCore.PlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.PlayablesAnimatorController.IsCurrentClipFinished() && !character.GraphCore.PlayablesAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));;
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.PlayablesAnimatorController.HasReachedActionTime() && character.StateTimer.ActionIsPossible())
        {
            character.Inventory.WeaponOff();
            character.GraphCore.PlayablesAnimatorController.ResetActionTimeFlag();
            character.StateTimer.SetActionIsPossible(false);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Inventory.ProjectileSystem.SetProjectileLoaded(false);
    }
}
