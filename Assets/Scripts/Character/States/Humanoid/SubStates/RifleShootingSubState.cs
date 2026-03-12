using UnityEngine;

[CreateAssetMenu(fileName = "RifleShootingSubState", menuName = "States/SubStates/RifleShootingSubState")]
public class RifleShootingSubState : State
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
        character.StateTimer.SetActionIsPossible(true);
        
        LoadAndShoot(character);
    }
    
    public override void FixedUpdateState(CharacterCore character)
    {
       
    }
    
    protected override void CheckSwitch(CharacterCore character)
    {
        if (character.GraphCore.UpperBodyLayerController.IsComplete())
        {
            character.SetSubState(character.StatesSet.GetState("RifleIdleAimSubState"));
        }
        
        if (character.Health.IsDestroyed)
        {
            character.SetSubState(character.StatesSet.GetState("DefaultSubState"));
        }
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.GraphCore.UpperBodyLayerController.HasReachedActionTime() && character.StateTimer.ActionIsPossible())
        {
            LoadAndShoot(character);
            character.StateTimer.SetActionIsPossible(false);
            character.GraphCore.UpperBodyLayerController.ResetActionTime();
        }
    }

    public override void ExitState(CharacterCore character)
    {
        if (!character.IsAI)
        {
            character.SceneCamera.SetCameraMode(CameraMode.FollowCamera);
        }
        character.CharacterInputHandler.ResetInputBuffer();
    }

    private void LoadAndShoot(CharacterCore character)
    {
        character.Inventory.ProjectileSystem.SetProjectileLoaded(true);
        character.Inventory.ProjectileSystem.TakeProjectile((WeaponData)character
            .Inventory.WeaponSystem.InstanceInHands.EquppiedItemData);
        character.Inventory.ProjectileSystem.Shot();
    }
}
