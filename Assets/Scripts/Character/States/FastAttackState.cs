using System.Linq;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "FastAttackState", menuName = "States/FastAttackState")]
public class FastAttackState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        var clipSet = Clips.FirstOrDefault(n => n.ParamValue == itemInstanceData.AnimationType);
        character.PlayablesAnimatorController.OnEnter(clipSet, EnterTransitionDuration);
        character.PlayablesAnimatorController.SetAnimationParameter(clipSet.ParameterName, character.AttackCounter.GetValue());
        
        character.Inventory.WeaponSystem.AllowAttack(true);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.PlayablesAnimatorController.IsBlendFinished())
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.Inventory.WeaponSystem.AllowAttack(false);
    }
}
