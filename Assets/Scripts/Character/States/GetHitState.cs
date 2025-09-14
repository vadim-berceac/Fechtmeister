using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "GetHitState", menuName = "States/GetHitState")]
public class GetHitState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.PlayablesAnimatorController.OnEnter(Clips[0], EnterTransitionDuration);
        
        character.Health.EnableHitReaction(false);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.CharacterInputHandler.IsWeaponDraw &&  character.PlayablesAnimatorController.IsBlendFinished())
        {
            character.SetState(character.StatesContainer.CombatIdleState);
        }
        
        if (!character.CharacterInputHandler.IsWeaponDraw &&  character.PlayablesAnimatorController.IsBlendFinished())
        {
            character.SetState(character.StatesContainer.IdleState);
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetHitState);
        }
    }
}
