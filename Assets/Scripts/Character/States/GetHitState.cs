using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "GetHitState", menuName = "States/GetHitState")]
public class GetHitState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => character.Health.IsDestroyed, "DeathState"),
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
            new(character => character.CharacterInputHandler.IsWeaponDraw 
                             && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "CombatIdleState"),
            new(character => !character.CharacterInputHandler.IsWeaponDraw 
                             && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "IdleState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands?.ItemData;
        var animType = character.Inventory.IsWeaponOn ? itemInstanceData.AnimationType : 0;
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, animType);
        character.GraphCore.FullBodyAnimatorController.SetAnimationStateClip(Random.Range(0, this.GetBlendAnimationsCount(animType)));
        
        character.Health.EnableHitReaction(false);
    }
}
