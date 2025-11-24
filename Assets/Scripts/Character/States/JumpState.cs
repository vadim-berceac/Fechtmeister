using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "JumpState", menuName = "States/JumpState")]
public class JumpState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => !character.Gravity.Grounded
                             && character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.5, "FallState"),
            new(character => character.Gravity.Grounded &&
                             character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.6, "LandingState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
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
        character.MoveLocal(character.CharacterInputHandler.DirVector3, character.CurrentSpeed.LastNotNullHorizontalSpeed);

        var normalizedTime = character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime();
       
        if (normalizedTime >= 0.4)
        {
            character.LedgeDetection.UpdateDetection(true, LedgeTypeDetection.High);
            Debug.Log(0);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.LedgeDetection.UpdateDetection(false, LedgeTypeDetection.High);
    }
}
