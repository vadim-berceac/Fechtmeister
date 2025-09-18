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
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.CharacterPlayablesAnimatorController.HasReachedActionTime())
        {
            character.Inventory.WeaponOn();
            character.CharacterPlayablesAnimatorController.ResetActionTimeFlag();
        }
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.CharacterPlayablesAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
    }
}
