using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatIdleState", menuName = "States/CombatIdleState")]
public class CombatIdleState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.StopPlayback();
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.IdleStateName, EnterTransitionDuration);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsWeaponDraw)
        {
            character.SetState(character.StatesContainer.WeaponOffState);
        }
        
        if (Mathf.Abs(character.CharacterInputHandler.InputX) > 0 ||
            Mathf.Abs(character.CharacterInputHandler.InputY) > 0)
        {
            character.SetState(character.StatesContainer.CombatWalkState);
        }

        if (character.CharacterInputHandler.IsAttack && !character.Inventory.WeaponSystem.WeaponInstanceIsRanged 
                                                     && character.Inventory.ProjectileSystem.ContainsProjectile())
        {
            character.SetState(character.StatesContainer.FastAttackState);
        }
        
        if (character.CharacterInputHandler.IsAimBlock && character.Inventory.WeaponSystem.WeaponInstanceIsRanged)
        {
            character.SetState(character.StatesContainer.LoadState);
        }
        
        if (character.CharacterInputHandler.IsJump)
        {
            character.SetState(character.StatesContainer.JumpState);
        }
        
        if (!character.Gravity.Grounded)
        {
            character.SetState(character.StatesContainer.FallState);
        }
    }
}
