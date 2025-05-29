using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "IdleState", menuName = "States/IdleState")]
public class IdleState : State
{
    public override void EnterState(CharacterCore character)
    {
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.IdleStateName, enterTransitionDuration, animationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (Mathf.Abs(character.CharacterInputHandler.InputX) > 0 ||
            Mathf.Abs(character.CharacterInputHandler.InputY) > 0)
        {
            character.SetState(character.StatesContainer.WalkState);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        
    }
}
