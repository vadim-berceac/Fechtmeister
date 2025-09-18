using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "FastAttackState", menuName = "States/FastAttackState")]
public class FastAttackState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
        character.CharacterPlayablesAnimatorController.SetAnimationStateClip(character.AttackCounter.GetValue());
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.CharacterPlayablesAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.CharacterPlayablesAnimatorController.HasReachedActionTime())
        {
            character.Inventory.WeaponSystem.AllowAttack(true);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Inventory.WeaponSystem.AllowAttack(false);
    }
}
