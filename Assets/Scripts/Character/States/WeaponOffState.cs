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
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.CharacterPlayablesAnimatorController.IsCurrentClipFinished() && !character.CharacterPlayablesAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));;
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.CharacterPlayablesAnimatorController.HasReachedActionTime())
        {
            character.Inventory.WeaponOff();
            character.CharacterPlayablesAnimatorController.ResetActionTimeFlag();
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Inventory.ProjectileSystem.SetProjectileLoaded(false);
    }
}
