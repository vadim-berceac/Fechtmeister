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
            new(character => character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "CombatIdleState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
        };
    }
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        
        character.SetAnimationByWeaponIndex(this);
        character.GraphCore.FullBodyAnimatorController.SetAnimationStateClip(character.AttackCounter.GetValue());
        ((WeaponInstance)character.Inventory.WeaponSystem.InstanceInHands).ResetAction();
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
