using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatWalkState", menuName = "States/CombatWalkState")]
public class CombatWalkState : MovementState
{
    protected override void CheckSwitch(CharacterCore character)
    {
        if (Mathf.Abs(character.CharacterInputHandler.InputX) == 0 &&
            Mathf.Abs(character.CharacterInputHandler.InputY) == 0)
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }

        if (character.CharacterInputHandler.IsRun)
        {
            character.SetState(character.StatesContainer.GetState("CombatRunState"));
        }

        if (!character.CharacterInputHandler.IsWeaponDraw)
        {
            character.SetState(character.StatesContainer.GetState("WeaponOffState"));
        }
        
        if (character.CharacterInputHandler.IsAimBlock && character.Inventory.WeaponSystem.WeaponInstanceIsRanged)
        {
            character.SetState(character.StatesContainer.GetState("LoadState"));
        }
        
        if (character.CharacterInputHandler.IsJump)
        {
            character.SetState(character.StatesContainer.GetState("JumpState"));
        }
        
        if (!character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.GetState("FallState"));
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
