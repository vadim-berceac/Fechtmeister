using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "FallDamageState", menuName = "States/FallDamageState")]
public class FallDamageState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.Health.EnableHitReaction(false);
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, 0);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.CharacterInputHandler.IsWeaponDraw && (Mathf.Abs(character.CharacterInputHandler.InputX) > 0 
                                                             || Mathf.Abs(character.CharacterInputHandler.InputY) > 0))
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
        if (!character.CharacterInputHandler.IsWeaponDraw && (Mathf.Abs(character.CharacterInputHandler.InputX) > 0 
                                                             || Mathf.Abs(character.CharacterInputHandler.InputY) > 0))
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
    }
}
