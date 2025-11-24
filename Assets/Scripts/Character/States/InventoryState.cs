using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "InventoryState", menuName = "States/InventoryState")]
public class InventoryState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => !character.CharacterInputHandler.IsInventoryOpen, "IdleState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
    }
}
