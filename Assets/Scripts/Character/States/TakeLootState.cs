using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "TakeLootState", menuName = "States/TakeLootState")]
public class TakeLootState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.CharacterPlayablesAnimatorController.SetAnimationState(this, 0);
        character.CharacterPlayablesAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Item));
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.CharacterPlayablesAnimatorController.IsCurrentClipFinished())
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
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
