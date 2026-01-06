using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "LoadState", menuName = "States/LoadState")]
public class LoadState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => !character.Inventory.ProjectileSystem.IsProjectileLoaded 
                             && character.Inventory.ProjectileSystem.HasProjectiles(), "ReloadProjectileState"),
            new(character => character.Gravity.Grounded && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished() 
                                                        && character.Inventory.ProjectileSystem.IsProjectileLoaded, "AimState"),
            new(character => !character.CharacterInputHandler.IsAimBlock , "CombatIdleState"),
            new(character => !character.Inventory.ProjectileSystem.HasProjectiles() 
                             && !character.Inventory.ProjectileSystem.IsProjectileLoaded, "GetHitState"), // заменить на какое-то новое состояние по типу получения хит реакции
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.SetAnimationByWeaponIndex(this);
    }
}
