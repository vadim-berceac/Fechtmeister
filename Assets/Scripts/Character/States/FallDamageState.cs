using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "FallDamageState", menuName = "States/FallDamageState")]
public class FallDamageState : State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.Health.EnableHitReaction(false);
        character.PlayablesAnimatorController.OnEnter(AnimationBlends[0], EnterTransitionDuration);
    }

    protected override void CheckSwitch(CharacterCore character)
    {
       
    }
}
