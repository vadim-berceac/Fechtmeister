using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "ReloadProjectileState", menuName = "States/ReloadProjectileState")]
public class ReloadProjectileState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        character.GraphCore.PlayablesAnimatorController.SetAnimationState(this, itemInstanceData.AnimationType);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.PlayablesAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Inventory.ProjectileSystem.SetProjectileLoaded(true);
    }
}
