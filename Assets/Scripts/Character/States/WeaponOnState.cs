using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "WeaponOnState", menuName = "States/WeaponOnState")]
public class WeaponOnState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        character.AttackCounter.SetValue(itemInstanceData.AttackCounterSettings.AttacksResetDelay, itemInstanceData.AttackCounterSettings.AttacksCount);
        character.GraphCore.PlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.PlayablesAnimatorController.HasReachedActionTime())
        {
            character.Inventory.WeaponOn();
            character.GraphCore.PlayablesAnimatorController.ResetActionTimeFlag();
        }
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.PlayablesAnimatorController.IsCurrentClipFinished() && !character.GraphCore.PlayablesAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
    }
}
