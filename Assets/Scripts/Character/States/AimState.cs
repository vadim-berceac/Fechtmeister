using System.Linq;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "AimState", menuName = "States/AimState")]
public class AimState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        var clipSet = Clips.FirstOrDefault(n => n.ParamValue == itemInstanceData.AnimationType);
        character.PlayablesAnimatorController.OnEnter(clipSet, EnterTransitionDuration);
        character.PlayablesAnimatorController.SetAnimationParameter(clipSet.ParameterName, character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsAimBlock)
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
        }
        
        if (character.CharacterInputHandler.IsAttack)
        {
            character.SetState(character.StatesContainer.GetState("ReleaseState"));
        }
    }
}
