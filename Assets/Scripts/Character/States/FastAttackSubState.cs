using Unity.Burst;
using UnityEngine;
using UnityEngine.Playables;

[BurstCompile]
[CreateAssetMenu(fileName = "FastAttackSubState", menuName = "States/SubStates/FastAttackSubState")]
public class FastAttackSubState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.LayerMixer.SetInputWeight(1,1f);
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData;
        
        character.GraphCore.UpperBodyLayerController.PlayAnimationSubState(this, itemInstanceData.AnimationType, character.AttackCounter.GetValue());
    }

    public override void FixedUpdateState(CharacterCore character)
    {
       
    }

    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.UpperBodyLayerController.IsComplete())
        {
            Debug.Log(character.GraphCore.UpperBodyLayerController);
            character.SetSubState(character.StatesContainer.GetState("DefaultSubState"));
        }
        
        // if (character.Health.IsHitReactionEnabled)
        // {
        //     character.SetSubState(character.StatesContainer.GetState("GetHitState"));
        // }
        //
        // if (character.Health.IsDestroyed)
        // {
        //     character.SetSubState(character.StatesContainer.GetState("DeathState"));
        // }
    }

    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.FullBodyAnimatorController.HasReachedActionTime() && character.StateTimer.ActionIsPossible())
        {
            character.Inventory.WeaponSystem.AllowAttack(true);
            character.GraphCore.FullBodyAnimatorController.ResetActionTimeFlag();
            character.StateTimer.SetActionIsPossible(false);
        }
    }
    
    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.GraphCore.UpperBodyLayerController.StopAnimationSubState();
        character.Inventory.WeaponSystem.AllowAttack(false);
    }
}
