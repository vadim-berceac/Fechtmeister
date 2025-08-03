using Unity.Burst;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryState", menuName = "States/InventoryState")]
public class InventoryState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.LocomotionSettings.Animator.CrossFade(AnimationParams.InventoryStateName, EnterTransitionDuration, AnimationLayer);
    }

    [BurstCompile]
    public override void UpdateState(CharacterCore character)
    {
        base.UpdateState(character);
        CheckSwitch(character);
    }

    public override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsInventoryOpen)
        {
            character.SetState(character.StatesContainer.IdleState);
        }
    }
}
