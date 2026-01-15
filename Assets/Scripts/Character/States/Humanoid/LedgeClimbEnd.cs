using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LedgeClimbEnd", menuName = "States/LedgeClimbEnd")]
public class LedgeClimbEnd: State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
            new(character => !character.CharacterInputHandler.IsWeaponDraw 
                             && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "IdleState"),
            new(character => character.CharacterInputHandler.IsWeaponDraw 
                             && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "CombatIdleState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
    }
}
