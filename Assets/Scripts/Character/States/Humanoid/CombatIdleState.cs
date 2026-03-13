using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatIdleState", menuName = "States/CombatIdleState")]
public class CombatIdleState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new (c => c.Health.IsDestroyed && c.IsBoss, "BossStunState"),
            new (c => c.Health.IsDestroyed && !c.IsBoss, "DeathState"),
            new (c => c.Health.IsHitReactionEnabled, "GetHitState"),
            new (character => !character.Inventory.IsWeaponOn, "IdleState"),
            new (c => Mathf.Abs(c.CharacterInputHandler.InputX) > 0 || Mathf.Abs(c.CharacterInputHandler.InputY) > 0, "CombatWalkState"),
            new (c => c.CharacterInputHandler.IsAttack && c.Inventory.WeaponSystem.RangeType != RangeTypes.Ranged 
                                                               && c.GraphCore.UpperBodyLayerController.IsComplete(), "FastAttackState"),
            new(character => character.CharacterInputHandler.IsAimBlock 
                             && character.Inventory.WeaponSystem.RangeType != RangeTypes.Melee &&
                             character.IsBoss, "LoadState"),
            new(character => character.CharacterInputHandler.IsAimBlock 
                             && character.Inventory.WeaponSystem.RangeType != RangeTypes.Melee &&
                             !character.Inventory.ProjectileSystem.IsProjectileLoaded 
                             && character.Inventory.ProjectileSystem.HasProjectiles()
                             && character.Inventory.WeaponSystem.AnimationType != 7,
                    "ReloadProjectileState"),
            new(character => character.CharacterInputHandler.IsAimBlock 
                             && character.Inventory.WeaponSystem.RangeType != RangeTypes.Melee &&
                             character.Inventory.ProjectileSystem.IsProjectileLoaded
                             && character.Inventory.WeaponSystem.AnimationType != 7, 
                    "LoadState"),
            new (c => c.CharacterInputHandler.IsJump, "JumpState"),
            new (c => !c.Gravity.Grounded, "FallState"),
        };
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.Inventory.WeaponSystem.CanUnDrawWeapon() 
            && character.GraphCore.UpperBodyLayerController.IsComplete())
        {
            character.SetSubState(character.StatesSet.GetState("WeaponOffSubState"));
        } 
    }
}
