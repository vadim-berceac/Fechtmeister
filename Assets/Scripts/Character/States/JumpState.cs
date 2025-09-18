using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "JumpState", menuName = "States/JumpState")]
public class JumpState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, 0);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.Gravity.Grounded
            && character.CharacterPlayablesAnimatorController.GetCurrentClipNormalizedTime() > 0.5)
        {
            character.SetState(character.StatesContainer.GetState("FallState"));
        }

        if (character.Gravity.Grounded &&
            character.CharacterPlayablesAnimatorController.GetCurrentClipNormalizedTime() > 0.6)
        {
            character.SetState(character.StatesContainer.GetState("LandingState"));
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        // не похоже, что работает
        character.MoveLocal(character.CharacterInputHandler.DirVector3 + character.CashedTransform.up, character.CurrentSpeed.LastNotNullHorizontalSpeed);
    }
}
