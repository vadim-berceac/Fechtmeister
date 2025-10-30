using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "FallState", menuName = "States/FallState")]
public class FallState :  State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.GetState("LandingState"));
        }

        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("FallDamageState"));
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
        character.LedgeDetection.UpdateDetection(true);
    }
}
