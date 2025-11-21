using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "ReleaseState", menuName = "States/ReleaseState")]
public class ReleaseState : State
{
    [field: SerializeField] private GameObject TestProjectile { get; set; }
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished(), "CombatIdleState"),
        };
    }
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.SetAnimationByWeaponIndex(this);
        character.GraphCore.FullBodyAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.GraphCore.FullBodyAnimatorController.BlendCurrentAnimationStateClips(character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        //character.Inventory.ProjectileSystem.Shot();
        var spawnPosition = character.transform.position + character.transform.forward * 0.5f + Vector3.up * 1.2f;
        var projectile = Instantiate(TestProjectile, spawnPosition, Quaternion.identity);
        projectile.GetComponent<Projectile>().Launch(character.LocomotionSettings.CharacterCollider,character.transform.forward);
    }
}
