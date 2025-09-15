using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "TakeLootState", menuName = "States/TakeLootState")]
public class TakeLootState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.PlayablesAnimatorController.OnEnter(AnimationBlends[0], EnterTransitionDuration);
        character.PlayablesAnimatorController.SetAnimationParameter(AnimationBlends[0].ParameterName, character.TargetingSystem.GetVerticalAngle(TargetingMode.Item));
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.PlayablesAnimatorController.IsBlendFinished())
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.PlayablesAnimatorController.SetAnimationParameter(AnimationBlends[0].ParameterName, character.TargetingSystem.GetVerticalAngle(TargetingMode.Item));
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        
        Take(character);
    }

    private static void Take(CharacterCore character)
    {
        var item = character.TargetingSystem.GetTargetItem();
        if (item == null)
        {
           return;
        }
        
        character.Inventory.AddToInventoryBag(item);
    }
}
