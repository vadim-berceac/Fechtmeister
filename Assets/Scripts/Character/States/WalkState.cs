using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "WalkState", menuName = "States/WalkState")]
public class WalkState : MovementState
{
    protected override void CheckSwitch(CharacterCore character)
    {
        if (Mathf.Abs(character.CharacterInputHandler.InputX) == 0 &&
            Mathf.Abs(character.CharacterInputHandler.InputY) == 0)
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }

        if (character.CharacterInputHandler.IsRun)
        {
            character.SetState(character.StatesContainer.GetState("RunState"));
        }

        if (character.CharacterInputHandler.IsWeaponDraw)
        {
            character.SetState(character.StatesContainer.GetState("WeaponOnState"));
        }
        
        if (character.CharacterInputHandler.IsJump)
        {
            character.SetState(character.StatesContainer.GetState("JumpState"));
        }
        
        if (!character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.GetState("FallState"));
        }
        
        if (character.CharacterInputHandler.IsInventoryOpen)
        {
            character.SetState(character.StatesContainer.GetState("InventoryState"));
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
        
        if (character.Health.IsDestroyed)
        {
            character.SetState(character.StatesContainer.GetState("DeathState"));
        }
    }
}