using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "ReloadProjectileState", menuName = "States/ReloadProjectileState")]
public class ReloadProjectileState : State
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
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.ShootingSystem.SetProjectileLoaded(true);
        character.ShootingSystem.TakeProjectile(TestProjectile);
    }
}
