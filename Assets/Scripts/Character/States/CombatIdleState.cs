using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatIdleState", menuName = "States/CombatIdleState")]
public class CombatIdleState : MovementState
{
    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsWeaponDraw && !character.GraphCore.FullBodyAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("WeaponOffState"));
        }
        
        if (Mathf.Abs(character.CharacterInputHandler.InputX) > 0 && !character.GraphCore.FullBodyAnimatorController.IsTransitioning ||
            Mathf.Abs(character.CharacterInputHandler.InputY) > 0 && !character.GraphCore.FullBodyAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("CombatWalkState"));
        }

        if (character.CharacterInputHandler.IsAttack && !character.Inventory.WeaponSystem.WeaponInstanceIsRanged 
                                                     && character.GraphCore.UpperBodyLayerController.IsComplete())
        {
            character.SetState(character.StatesContainer.GetState("FastAttackState"));
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
