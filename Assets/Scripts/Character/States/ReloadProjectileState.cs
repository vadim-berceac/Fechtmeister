using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "ReloadProjectileState", menuName = "States/ReloadProjectileState")]
public class ReloadProjectileState : State
{
    [field: SerializeField] private ProjectileData ProjectileData { get; set; }
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "CombatIdleState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.SetAnimationByWeaponIndex(this);
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.FullBodyAnimatorController.HasReachedActionTime() && character.StateTimer.ActionIsPossible())
        {
            
            character.ShootingSystem.SetProjectileLoaded(true);
            character.ShootingSystem.TakeProjectile(ProjectileData);
            character.GraphCore.FullBodyAnimatorController.ResetActionTimeFlag();
            character.StateTimer.SetActionIsPossible(false);
        }
    }
}
