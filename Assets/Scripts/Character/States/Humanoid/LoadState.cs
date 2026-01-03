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
            new(character => !character.Inventory.ProjectileSystem.IsProjectileLoaded, "ReloadProjectileState"),
            new(character => character.Gravity.Grounded && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "AimState"),
            new(character => !character.CharacterInputHandler.IsAimBlock, "CombatIdleState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.SetAnimationByWeaponIndex(this);
    }
}
