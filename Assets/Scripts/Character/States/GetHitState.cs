using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "GetHitState", menuName = "States/GetHitState")]
public class GetHitState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, 0);
        
        character.Health.EnableHitReaction(false);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.CharacterInputHandler.IsWeaponDraw && character.CharacterPlayablesAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
        
        if (!character.CharacterInputHandler.IsWeaponDraw && character.CharacterPlayablesAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
    }
}
