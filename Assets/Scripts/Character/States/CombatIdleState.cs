using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatIdleState", menuName = "States/CombatIdleState")]
public class CombatIdleState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsWeaponDraw && !character.CharacterPlayablesAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("WeaponOffState"));
        }
        
        if (Mathf.Abs(character.CharacterInputHandler.InputX) > 0 && !character.CharacterPlayablesAnimatorController.IsTransitioning ||
            Mathf.Abs(character.CharacterInputHandler.InputY) > 0 && !character.CharacterPlayablesAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("CombatWalkState"));
        }

        if (character.CharacterInputHandler.IsAttack && !character.Inventory.WeaponSystem.WeaponInstanceIsRanged 
                                                     && character.Inventory.ProjectileSystem.ContainsProjectile())
        {
            character.SetState(character.StatesContainer.GetState("FastAttackState"));
        }
        
        if (character.CharacterInputHandler.IsAimBlock && character.Inventory.WeaponSystem.WeaponInstanceIsRanged)
        {
            character.SetState(character.StatesContainer.GetState("LoadState"));
        }
        
        if (character.CharacterInputHandler.IsJump)
        {
            character.SetState(character.StatesContainer.GetState("JumpState"));
        }
        
        if (!character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.GetState("FallState"));
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
    }
}
