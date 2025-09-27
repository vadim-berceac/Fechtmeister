using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "DeathState", menuName = "States/DeathState")]
public class DeathState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands?.ItemData;
        var animType = character.Inventory.IsWeaponOn ? itemInstanceData.AnimationType : 0;
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, animType);
        character.GraphCore.FullBodyAnimatorController.SetAnimationStateClip(Random.Range(0, this.GetBlendAnimationsCount(animType)));
        
        character.Health.EnableHitReaction(false);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
       
    }
}
