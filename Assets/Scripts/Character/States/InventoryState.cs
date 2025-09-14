using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "InventoryState", menuName = "States/InventoryState")]
public class InventoryState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        //character.PlayablesAnimatorController.OnEnter(Clips[0], EnterTransitionDuration);
        character.PlayablesAnimatorController.Stop();
        character.CharacterPlayablesAnimatorController.SelectAnimationState(name);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsInventoryOpen)
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
    }
}
