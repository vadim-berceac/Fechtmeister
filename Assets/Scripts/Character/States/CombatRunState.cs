using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "CombatRunState", menuName = "States/CombatRunState")]
public class CombatRunState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands?.ItemData;
        character.GraphCore.PlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (Mathf.Abs(character.CharacterInputHandler.InputX) == 0 &&
            Mathf.Abs(character.CharacterInputHandler.InputY) == 0)
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
        
        if (!character.CharacterInputHandler.IsRun && !character.GraphCore.PlayablesAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("CombatWalkState"));
        }
        
        if (!character.CharacterInputHandler.IsWeaponDraw && !character.GraphCore.PlayablesAnimatorController.IsTransitioning)
        {
            character.SetState(character.StatesContainer.GetState("WeaponOffState"));
        }
        
        if (character.CharacterInputHandler.IsAttack && !character.Inventory.WeaponSystem.WeaponInstanceIsRanged)
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
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.GraphCore.PlayablesAnimatorController.Move(character.CharacterInputHandler.InputX, character.CharacterInputHandler.InputY);
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.CurrentSpeed.StopUpdateLastHorizontalSpeed();
    }
}
