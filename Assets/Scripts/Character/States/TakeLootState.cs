using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "TakeLootState", menuName = "States/TakeLootState")]
public class TakeLootState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.TakeLootStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (character.LocomotionSettings.Animator.GetFloat(AnimationParams.OneShotPlayed) == 0)
        {
            character.SetState(character.StatesContainer.IdleState);
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
