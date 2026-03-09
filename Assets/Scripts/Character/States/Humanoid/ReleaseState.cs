using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "ReleaseState", menuName = "States/ReleaseState")]
public class ReleaseState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => character.Health.IsHitReactionEnabled, "GetHitState"),
            new(character => character.CharacterInputHandler.IsAimBlock 
                             && character.Inventory.WeaponSystem.RangeType != RangeTypes.Melee &&
                             character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished() &&
                             !character.Inventory.ProjectileSystem.IsProjectileLoaded 
                              && character.Inventory.ProjectileSystem.HasProjectiles(), "ReloadProjectileState"),
            new(character => character.CharacterInputHandler.IsAimBlock 
                             && character.Inventory.WeaponSystem.RangeType != RangeTypes.Melee &&
                             character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished() &&
                             character.Inventory.ProjectileSystem.IsProjectileLoaded, "LoadState"),
            new(character => character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "CombatIdleState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.SetAnimationByWeaponIndex(this);
        character.Inventory.ProjectileSystem.Shot();
        if (!character.IsAI)
        {
            character.SceneCamera.SetCameraMode(CameraMode.AimCamera);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        if (!character.IsAI)
        {
            character.SceneCamera.SetCameraMode(CameraMode.FollowCamera);
        }
    }
}
