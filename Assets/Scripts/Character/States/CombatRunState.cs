using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatRunState", menuName = "States/CombatRunState")]
public class CombatRunState : MovementState
{
    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsRun && !character.GraphCore.FullBodyAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("CombatWalkState"));
        }
        
        if (!character.CharacterInputHandler.IsWeaponDraw && !character.GraphCore.FullBodyAnimatorController.IsTransitioning)
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
        
        if (character.Gravity.Grounded && character.StateTimer.GetCurrentTimeInState() > 5f)
        {
            character.SetState(character.StatesContainer.GetState("CombatSprintState"));
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
