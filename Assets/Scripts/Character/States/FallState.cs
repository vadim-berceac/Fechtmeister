using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "FallState", menuName = "States/FallState")]
public class FallState :  State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.PlayablesAnimatorController.SetAnimationState(this, 0);
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
    }
}
