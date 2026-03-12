using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "DefaultSubState", menuName = "States/SubStates/DefaultSubState")]
public class DefaultSubState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.UpperBodyLayerController.PlayAnimationSubState(this, 
            0, 0);
        character.GraphCore.UpperBodyLayerController.StopAnimationSubState();
    }
    
    public override void FixedUpdateState(CharacterCore character)
    {
       
    }

    protected override void CheckSwitch(CharacterCore character)
    {
       
    }

    protected override void CheckAction(CharacterCore character)
    {
        if (character.GraphCore.UpperBodyLayerController.IsComplete() && character.Inventory.IsWeaponOn
            && character.Inventory.WeaponSystem.AnimationType == 7
            && !character.Health.IsDestroyed)
        {
            character.SetSubState(character.StatesSet.GetState("RifleIdleAimSubState"));
        }
    }
}
