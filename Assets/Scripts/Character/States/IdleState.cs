using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "IdleState", menuName = "States/IdleState")]
public class IdleState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, 0);
    }
    
    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (Mathf.Abs(character.CharacterInputHandler.InputX) > 0 ||
            Mathf.Abs(character.CharacterInputHandler.InputY) > 0)
        {
            character.SetState(character.StatesContainer.GetState("WalkState"));
        }
        
        if (character.CharacterInputHandler.IsWeaponDraw)
        {
            character.SetState(character.StatesContainer.GetState("WeaponOnState"));
        }

        if (character.CharacterInputHandler.IsJump)
        {
            character.CurrentSpeed.StopUpdateLastHorizontalSpeed();
            character.SetState(character.StatesContainer.GetState("JumpState"));
        }

        if (!character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.GetState("FallState"));
        }
        
        if (character.CharacterInputHandler.IsInteract && character.TargetingSystem.HasTarget(TargetingMode.Item))
        {
            character.SetState(character.StatesContainer.GetState("TakeLootState"));
        }
        
        if (character.CharacterInputHandler.IsInventoryOpen)
        {
            character.SetState(character.StatesContainer.GetState("InventoryState"));
        }

        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.CharacterInputHandler.ResetInteract();
    }
}
