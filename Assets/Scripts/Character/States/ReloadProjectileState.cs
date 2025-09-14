using System.Linq;
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
        var clipSet = Clips.FirstOrDefault(n => n.ParamValue == itemInstanceData.AnimationType);
        character.PlayablesAnimatorController.OnEnter(clipSet, EnterTransitionDuration);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.PlayablesAnimatorController.IsBlendFinished())
        {
            character.SetState(character.StatesContainer.CombatIdleState);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Inventory.ProjectileSystem.SetProjectileLoaded(true);
    }
}
