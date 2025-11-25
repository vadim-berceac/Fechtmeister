using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "LedgeClimbState", menuName = "States/LedgeClimbState")]
public class LedgeClimbState: State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "LedgeClimbEnd"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
        };
    }
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
        character.FaceWallNormal(character.LedgeDetection.LastWallNormal);
    }
    
    public override void FixedUpdateState(CharacterCore character)
    {
        base.FixedUpdateState(character);
        
        if (character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.4f)
        {
            character.MoveToLedgeBlended(1.2f);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.LedgeDetection.Reset();
    }
}
