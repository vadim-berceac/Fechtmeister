using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "FallState", menuName = "States/FallState")]
public class FallState :  State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => character.Gravity.Grounded, "LandingState"),
            new(character => character.Health.IsHitReactionEnabled, "FallDamageState"),
            new(character => character.Health.IsDestroyed, "DeathState"),
            new(character => character.LedgeDetection.LedgeGrabPoint != Vector3.zero &&
                             character.CharacterInputHandler.TargetInputMagnitude > 0f, "LedgeClimbState"),
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
        character.LedgeDetection.UpdateDetection(true, LedgeTypeDetection.High);
    }
    
    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.LedgeDetection.UpdateDetection(false, LedgeTypeDetection.High);
    }
}
