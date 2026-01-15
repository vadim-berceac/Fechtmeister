using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "SprintStopState", menuName = "States/SprintStopState")]
public class SprintStopState: State
{
    [field: SerializeField] private AnimationCurve Curve { get; set; }
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => (character.Health.IsHitReactionEnabled), "GetHitState"),
            new(character => (!character.CharacterInputHandler.IsWeaponDraw 
                              && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished()), "IdleState"),
            new(character => (character.CharacterInputHandler.IsWeaponDraw 
                              && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished()), "CombatIdleState"),
            new(character => (!character.CharacterInputHandler.IsWeaponDraw 
                              && (character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime()) > 0.5f)
                              && (character.CharacterInputHandler.InputY > 0), "WalkState"),
            new(character => (character.CharacterInputHandler.IsWeaponDraw  
                              && (character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime()) > 0.5f)
                              && (character.CharacterInputHandler.InputY > 0), "CombatWalkState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.MoveLocal(character.CashedTransform.forward, Time.deltaTime 
                                                               * Curve.Evaluate(character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime()));
    }
}
