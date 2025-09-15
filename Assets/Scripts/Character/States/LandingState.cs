using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "LandingState", menuName = "States/LandingState")]
public class LandingState: State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.PlayablesAnimatorController.OnEnter(AnimationBlends[0], EnterTransitionDuration);
    }
    
    protected override void CheckSwitch(CharacterCore character)
    {
         if (!character.CharacterInputHandler.IsWeaponDraw && character.PlayablesAnimatorController.IsBlendFinished())
         {
             character.SetState(character.StatesContainer.GetState("IdleState"));
         }
        
         if (character.CharacterInputHandler.IsWeaponDraw && character.PlayablesAnimatorController.IsBlendFinished())
         {
             character.SetState(character.StatesContainer.GetState("CombatIdleState"));
         }
    }
}