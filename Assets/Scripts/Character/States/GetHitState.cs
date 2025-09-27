using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "GetHitState", menuName = "States/GetHitState")]
public class GetHitState : State
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
        if (character.Health.IsDestroyed)
        {
            character.SetState(character.StatesContainer.GetState("DeathState"));
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
        
        if (character.CharacterInputHandler.IsWeaponDraw && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
        
        if (!character.CharacterInputHandler.IsWeaponDraw && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
    }
}
