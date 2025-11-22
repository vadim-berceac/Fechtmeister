using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "LoadState", menuName = "States/LoadState")]
public class LoadState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => !character.ShootingSystem.IsProjectileLoaded, "ReloadProjectileState"),
            new(character => character.Gravity.Grounded && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "AimState"),
            new(character => !character.CharacterInputHandler.IsAimBlock, "CombatIdleState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.SetAnimationByWeaponIndex(this);
        character.GraphCore.FullBodyAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.GraphCore.FullBodyAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }
}
