using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatSprintState", menuName = "States/CombatSprintState")]
public class CombatSprintState : MovementState
{
    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsRun)
        {
            character.SetState(character.StatesContainer.GetState("CombatRunState"));
        }
        
        if (character.CharacterInputHandler.TargetInputMagnitude < 0.2f)
        {
            character.SetState(character.StatesContainer.GetState("SprintStopState"));
        }
        
        if (!character.CharacterInputHandler.IsWeaponDraw && !character.GraphCore.FullBodyAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("WeaponOffState"));
        }
        
        if (character.CharacterInputHandler.IsJump)
        {
            character.SetState(character.StatesContainer.GetState("JumpState"));
        }
        
        if (!character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.GetState("FallState"));
        }
        
        if (character.CharacterInputHandler.IsAimBlock && character.Inventory.WeaponSystem.WeaponInstanceIsRanged)
        {
            character.SetState(character.StatesContainer.GetState("LoadState"));
        }
        
        if (character.CharacterInputHandler.IsJump)
        {
            character.SetState(character.StatesContainer.GetState("JumpState"));
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
