using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "JumpState", menuName = "States/JumpState")]
public class JumpState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.Gravity.Grounded
            && character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.5)
        {
            character.SetState(character.StatesContainer.GetState("FallState"));
        }

        if (character.Gravity.Grounded &&
            character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.6)
        {
            character.SetState(character.StatesContainer.GetState("LandingState"));
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
        
        if (character.Health.IsDestroyed)
        {
            character.SetState(character.StatesContainer.GetState("DeathState"));
        }

        if (character.LedgeDetection.LedgeGrabPoint != Vector3.zero &&
            character.CharacterInputHandler.TargetInputMagnitude > 0f)
        {
            character.SetState(character.StatesContainer.GetState("LedgeClimbState"));
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.MoveLocal(character.CharacterInputHandler.DirVector3, character.CurrentSpeed.LastNotNullHorizontalSpeed);

        if (character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.4)
        {
            character.LedgeDetection.UpdateDetection(true);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.LedgeDetection.UpdateDetection(false);
    }
}
