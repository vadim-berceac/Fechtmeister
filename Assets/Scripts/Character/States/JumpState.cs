using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "JumpState", menuName = "States/JumpState")]
public class JumpState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.PlayablesAnimatorController.SetAnimationState(this, 0);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.Gravity.Grounded
            && character.GraphCore.PlayablesAnimatorController.GetCurrentClipNormalizedTime() > 0.5)
        {
            character.SetState(character.StatesContainer.GetState("FallState"));
        }

        if (character.Gravity.Grounded &&
            character.GraphCore.PlayablesAnimatorController.GetCurrentClipNormalizedTime() > 0.6)
        {
            character.SetState(character.StatesContainer.GetState("LandingState"));
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.MoveLocal(character.CharacterInputHandler.DirVector3, character.CurrentSpeed.LastNotNullHorizontalSpeed);
    }
}
