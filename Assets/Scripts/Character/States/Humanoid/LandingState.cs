using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "LandingState", menuName = "States/LandingState")]
public class LandingState: State
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
            new(character => !character.CharacterInputHandler.IsWeaponDraw && character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.75
                                                                           && character.CurrentSpeed.LastNotNullHorizontalSpeed > 4 
                                                                           && character.CharacterInputHandler.IsRun, "RunState"),
            new(character => character.CharacterInputHandler.IsWeaponDraw && character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.75
                                                                          && character.CurrentSpeed.LastNotNullHorizontalSpeed > 4 
                                                                          && character.CharacterInputHandler.IsRun, "CombatRunState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var param = character.CurrentSpeed.LastNotNullHorizontalSpeed > 4 ? 1 : 0;
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, param);
    }
}