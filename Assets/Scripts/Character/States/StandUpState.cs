using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "StandUpState", menuName = "States/StandUpState")]
public class StandUpState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, 0);
    }
    
    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsWeaponDraw && character.CharacterPlayablesAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
         
        if (character.CharacterInputHandler.IsWeaponDraw && character.CharacterPlayablesAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
    }
}
