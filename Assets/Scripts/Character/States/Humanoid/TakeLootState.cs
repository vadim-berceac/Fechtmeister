using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "TakeLootState", menuName = "States/TakeLootState")]
public class TakeLootState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => (character.Health.IsHitReactionEnabled), "GetHitState"),
            new(character => (character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished()), "IdleState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.CharacterInputHandler.ResetInputBuffer();
        character.SetAnimationByWeaponIndex(this);
        character.GraphCore.FullBodyAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Item));
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.GraphCore.FullBodyAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Item));
    }
    
    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Take();
    }
}
