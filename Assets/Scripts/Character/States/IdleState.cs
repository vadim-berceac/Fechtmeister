using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "IdleState", menuName = "States/IdleState")]
public class IdleState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.IdleStateName, EnterTransitionDuration, AnimationLayer);
        character.LocomotionSettings.Animator.SetFloat(AnimationParams.Speed, 0);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (Mathf.Abs(character.CharacterInputHandler.InputX) > 0 ||
            Mathf.Abs(character.CharacterInputHandler.InputY) > 0)
        {
            character.SetState(character.StatesContainer.WalkState);
        }
        
        if (character.CharacterInputHandler.IsWeaponDraw)
        {
            character.SetState(character.StatesContainer.WeaponOnState);
        }

        if (character.CharacterInputHandler.IsJump)
        {
            character.SetState(character.StatesContainer.JumpState);
        }

        if (!character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.FallState);
        }
        
        if (character.CharacterInputHandler.IsInteract && character.TargetingSystem.HasTarget(TargetingMode.Item))
        {
            character.SetState(character.StatesContainer.TakeLootState);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.CharacterInputHandler.ResetInteract();
    }
}
