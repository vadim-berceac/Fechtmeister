using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "TakeLootState", menuName = "States/TakeLootState")]
public class TakeLootState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
        character.GraphCore.FullBodyAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Item));
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.GraphCore.FullBodyAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Item));
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
