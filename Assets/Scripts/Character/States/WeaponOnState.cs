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
        character.PlayablesAnimatorController.OnEnter(Clips[0], EnterTransitionDuration);
        character.PlayablesAnimatorController.SetAnimationParameter(Clips[0].ParameterName, itemInstanceData.AnimationType);
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.PlayablesAnimatorController.IsActionEnabled)
        {
            character.Inventory.WeaponOn();
            character.PlayablesAnimatorController.ResetActionFlag();
        }
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.PlayablesAnimatorController.IsBlendFinished())
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
    }
}
