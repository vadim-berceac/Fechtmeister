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
        character.GraphCore.PlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
        character.GraphCore.PlayablesAnimatorController.SetAnimationStateClip(character.AttackCounter.GetValue());
        
        //тест
        character.GraphCore.PlayablesAnimationStateController.SetAnimationState(name, itemInstanceData.AnimationType, character.AttackCounter.GetValue());
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.PlayablesAnimatorController.IsCurrentClipFinished() && !character.GraphCore.PlayablesAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
        
        if (character.Health.IsDestroyed)
        {
            character.SetState(character.StatesContainer.GetState("DeathState"));
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.PlayablesAnimatorController.HasReachedActionTime() && character.StateTimer.ActionIsPossible())
        {
            character.Inventory.WeaponSystem.AllowAttack(true);
            character.GraphCore.PlayablesAnimatorController.ResetActionTimeFlag();
            character.StateTimer.SetActionIsPossible(false);
        }
    }
    
    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Inventory.WeaponSystem.AllowAttack(false);
    }
}
