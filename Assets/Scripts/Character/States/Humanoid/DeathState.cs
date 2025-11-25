using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "DeathState", menuName = "States/DeathState")]
public class DeathState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands?.EquppiedItemData;
        var animType = character.Inventory.IsWeaponOn ? itemInstanceData.AnimationType : 0;
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, animType);
        character.GraphCore.FullBodyAnimatorController.SetAnimationStateClip(Random.Range(0, this.GetBlendAnimationsCount(animType)));
        //character.CharacterColliderSizer.SetEnabled(false);
        character.Health.EnableHitReaction(false);
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.FullBodyAnimatorController.HasReachedActionTime())
        {
            character.CharacterColliderSizer.SetEnabled(false);
            character.GraphCore.FullBodyAnimatorController.ResetActionTimeFlag();
            character.StateTimer.SetActionIsPossible(false);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.CharacterColliderSizer.SetEnabled(true);
    }
}
