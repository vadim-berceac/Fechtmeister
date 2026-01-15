using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "FallDamageState", menuName = "States/FallDamageState")]
public class FallDamageState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => character.Health.IsDestroyed, "DeathState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
            new(character => Mathf.Abs(character.CharacterInputHandler.InputX) > 0 && character.StateTimer.GetCurrentTimeInState() > 3
                             || Mathf.Abs(character.CharacterInputHandler.InputY) > 0 && character.StateTimer.GetCurrentTimeInState() > 3, "StandUpState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
    }
}
