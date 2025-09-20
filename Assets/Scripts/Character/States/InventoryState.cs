using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "InventoryState", menuName = "States/InventoryState")]
public class InventoryState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.PlayablesAnimatorController.SetAnimationState(this, 0);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (!character.CharacterInputHandler.IsInventoryOpen)
        {
            character.SetState(character.StatesContainer.GetState("IdleState"));
        }
    }
}
