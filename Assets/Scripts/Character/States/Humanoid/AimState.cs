using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "AimState", menuName = "States/AimState")]
public class AimState : State
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new (c => c.Health.IsDestroyed, "DeathState"),
            new(c => c.Health.IsHitReactionEnabled, "GetHitState"),
            new (c => !c.CharacterInputHandler.IsAimBlock, "CombatIdleState"),
            new(c => c.CharacterInputHandler.IsAttack, "ReleaseState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.SetAnimationByWeaponIndex(this);
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
