using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "FallDamageState", menuName = "States/FallDamageState")]
public class FallDamageState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.Health.EnableHitReaction(false);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.FullBodyAnimatorController.IsTransitioning)
        {
            return;
        }
        
        if (character.Health.IsDestroyed)
        {
            character.SetState(character.StatesContainer.GetState("DeathState"));
        }
        
        if ( (Mathf.Abs(character.CharacterInputHandler.InputX) > 0 || Mathf.Abs(character.CharacterInputHandler.InputY) > 0))
        {
            if (character.StateTimer.GetCurrentTimeInState() > 3)
            {
                character.SetState(character.StatesContainer.GetState("StandUpState"));
            }
        }
    }
}
