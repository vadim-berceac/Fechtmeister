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
            new (c => c.Health.IsHitReactionEnabled, "GetHitState"),
            new (character => !character.Inventory.IsWeaponOn, "IdleState"),
            new (c => Mathf.Abs(c.CharacterInputHandler.InputX) > 0 || Mathf.Abs(c.CharacterInputHandler.InputY) > 0, "CombatWalkState"),
            new (c => c.CharacterInputHandler.IsAttack && !c.Inventory.WeaponSystem.WeaponInstanceIsRanged 
                                                               && c.GraphCore.UpperBodyLayerController.IsComplete(), "FastAttackState"),
            new(character => character.CharacterInputHandler.IsAimBlock 
                             && character.Inventory.WeaponSystem.WeaponInstanceIsRanged &&
                             character.IsBoss, "LoadState"),
            new(character => character.CharacterInputHandler.IsAimBlock 
                             && character.Inventory.WeaponSystem.WeaponInstanceIsRanged &&
                             !character.Inventory.ProjectileSystem.IsProjectileLoaded 
                             && character.Inventory.ProjectileSystem.HasProjectiles(), "ReloadProjectileState"),
            new(character => character.CharacterInputHandler.IsAimBlock 
                             && character.Inventory.WeaponSystem.WeaponInstanceIsRanged &&
                             character.Inventory.ProjectileSystem.IsProjectileLoaded, "LoadState"),
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
