using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "StandUpState", menuName = "States/StandUpState")]
public class StandUpState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => (!character.CharacterInputHandler.IsWeaponDraw 
                              && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished()), "IdleState"),
            new(character => (character.CharacterInputHandler.IsWeaponDraw 
                              && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished()), "CombatIdleState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
    }
}
