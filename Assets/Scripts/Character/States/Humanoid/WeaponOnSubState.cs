using UnityEngine;

[CreateAssetMenu(fileName = "WeaponOnSubState", menuName = "States/SubStates/WeaponOnSubState")]
public class WeaponOnSubState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.EquppiedItemData;
        
        character.GraphCore.UpperBodyLayerController.PlayAnimationSubState(this, 
            itemInstanceData.AnimationType, 0);
        ((WeaponInstance)character.Inventory.WeaponSystem.InstanceInHands).ResetAction();
        
        character.AttackCounter.SetValue(itemInstanceData.AttackCounterSettings.AttacksResetDelay,
            itemInstanceData.AttackCounterSettings.AttacksCount);
    }

    public override void FixedUpdateState(CharacterCore character){}

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.UpperBodyLayerController.IsComplete())
        {
            character.SetSubState(character.StatesSet.GetState("DefaultSubState"));
        }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.UpperBodyLayerController.HasReachedActionTime() && character.StateTimer.ActionIsPossible())
        {
            character.Inventory.WeaponOn();
            character.StateTimer.SetActionIsPossible(false);
            character.GraphCore.UpperBodyLayerController.ResetActionTime();
        }

        if (character.GraphCore.UpperBodyLayerController.GetCurrentClipNormalizedTime() > 0.6f)
        {
            character.GraphCore.UpperBodyLayerController.ModifyCurrentWeight(- Time.deltaTime * 2);
        }
    }
    
    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        //character.GraphCore.UpperBodyLayerController.StopAnimationSubState();
        character.Inventory.WeaponOn();
    }
}
