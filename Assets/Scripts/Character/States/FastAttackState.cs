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
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
        character.GraphCore.FullBodyAnimatorController.SetAnimationStateClip(character.AttackCounter.GetValue());
        ((WeaponInstance)character.Inventory.WeaponSystem.InstanceInHands).ResetAction();
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished() && !character.GraphCore.FullBodyAnimatorController.IsTransitioning)
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
        if (character.GraphCore.FullBodyAnimatorController.HasReachedActionTime() && character.StateTimer.ActionIsPossible())
        {
            character.Inventory.WeaponSystem.InstanceInHands.ItemControlComponent.Use();
            character.StateTimer.SetActionIsPossible(false);
            character.GraphCore.FullBodyAnimatorController.ResetActionTimeFlag();
        }
    }
}
