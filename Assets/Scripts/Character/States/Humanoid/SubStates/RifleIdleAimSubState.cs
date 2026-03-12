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

    public override void ExitState(CharacterCore character)
    {
        if (!character.IsAI)
        {
            character.SceneCamera.SetCameraMode(CameraMode.FollowCamera);
        }
    }
}
