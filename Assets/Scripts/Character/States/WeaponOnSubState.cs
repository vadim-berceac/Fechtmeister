using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "WeaponOnSubState", menuName = "States/SubStates/WeaponOnSubState")]
public class WeaponOnSubState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        
        character.GraphCore.UpperBodyLayerController.PlayAnimationSubState(this, 
            itemInstanceData.AnimationType, 0, EnterTransitionDuration);
    }
    
    public override void FixedUpdateState(CharacterCore character)
    {
       
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.UpperBodyLayerController.HasReachedActionTime() && character.StateTimer.ActionIsPossible())
        {
            character.Inventory.WeaponOn();
            character.GraphCore.UpperBodyLayerController.ResetActionTimeFlag();
            character.StateTimer.SetActionIsPossible(false);
        }
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.UpperBodyLayerController.IsComplete())
        {
            character.SetState(character.StatesContainer.GetState("CombatIdleState"));
            character.SetSubState(character.StatesContainer.GetState("DefaultSubState"));
        }
    }
    
    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.GraphCore.UpperBodyLayerController.StopAnimationSubState();
    }
}
