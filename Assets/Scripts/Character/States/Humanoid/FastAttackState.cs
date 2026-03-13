using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "FastAttackState", menuName = "States/FastAttackState")]
public class FastAttackState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new (c => c.Health.IsDestroyed && c.IsBoss, "BossStunState"),
            new (c => c.Health.IsDestroyed && !c.IsBoss, "DeathState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
            new(character => character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "CombatIdleState"),
        };
    }
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        
        character.SetAnimationByWeaponIndex(this);
        character.GraphCore.FullBodyAnimatorController.SetAnimationStateClip(character.AttackCounter.GetValue());
        ((WeaponInstance)character.Inventory.WeaponSystem.InstanceInHands).ResetAction();
        
        character.GraphCore.UpperBodyLayerController.ResetActionTime();
        character.Inventory.ProjectileSystem.ReturnProjectile((WeaponData)character.
            Inventory.WeaponSystem.InstanceInHands.EquppiedItemData);
        character.Inventory.ProjectileSystem.SetProjectileLoaded(false);
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.FullBodyAnimatorController.HasReachedActionTime() && character.StateTimer.ActionIsPossible())
        {
            if(character.IsBoss) ActionTime?.Invoke();
            character.Inventory.WeaponSystem.InstanceInHands.ItemControlComponent.Use();
            character.StateTimer.SetActionIsPossible(false);
            character.GraphCore.FullBodyAnimatorController.ResetActionTimeFlag();
        }
    }
}
