using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "WeaponOffState", menuName = "States/WeaponOffState")]
public class WeaponOffState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => (character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished()), "IdleState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.EquppiedItemData;
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.FullBodyAnimatorController.HasReachedActionTime() && character.StateTimer.ActionIsPossible())
        {
            character.Inventory.WeaponOff();
            character.GraphCore.FullBodyAnimatorController.ResetActionTimeFlag();
            character.StateTimer.SetActionIsPossible(false);
            character.Inventory.ProjectileSystem.ReturnProjectile();
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Inventory.ProjectileSystem.SetProjectileLoaded(false);
    }
}
