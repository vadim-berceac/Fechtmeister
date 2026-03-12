using UnityEngine;

[CreateAssetMenu(fileName = "RifleIdleAimSubState", menuName = "States/SubStates/RifleIdleAimSubState")]
public class RifleIdleAimSubState : State
{
    public override void EnterState(CharacterCore character)
    {
        var itemInstanceData = (WeaponData)character.Inventory.WeaponSystem.InstanceInHands.EquppiedItemData;
        character.GraphCore.UpperBodyLayerController.PlayAnimationSubState(this, 
            itemInstanceData.AnimationType, 0);
        
        if (!character.IsAI)
        {
            character.SceneCamera.SetCameraMode(CameraMode.AimCamera);
        }
    }
    
    public override void FixedUpdateState(CharacterCore character)
    {
       
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        if (
            character.Inventory.IsWeaponOn 
            && character.Inventory.WeaponSystem.RangeType != RangeTypes.Melee
            && character.CharacterInputHandler.IsAttack
            && !character.Health.IsDestroyed
            )
        {
            character.SetSubState(character.StatesSet.GetState("RifleShootingSubState"));
        }

        if (character.Health.IsDestroyed)
        {
            character.SetSubState(character.StatesSet.GetState("DefaultSubState"));
        }
    }

    public override void ExitState(CharacterCore character)
    {
        if (!character.IsAI)
        {
            character.SceneCamera.SetCameraMode(CameraMode.FollowCamera);
        }
    }
}
