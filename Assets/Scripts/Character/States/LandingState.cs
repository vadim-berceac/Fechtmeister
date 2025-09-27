using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "LandingState", menuName = "States/LandingState")]
public class LandingState: State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
    }
    
    protected override void CheckSwitch(CharacterCore character)
    {
         if (!character.CharacterInputHandler.IsWeaponDraw && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished())
         {
             character.SetState(character.StatesContainer.GetState("IdleState"));
         }
         
         if (character.CharacterInputHandler.IsWeaponDraw && character.GraphCore.FullBodyAnimatorController.IsCurrentClipFinished())
         {
             character.SetState(character.StatesContainer.GetState("CombatIdleState"));
         }
         
         if (character.Health.IsDestroyed)
         {
             character.SetState(character.StatesContainer.GetState("DeathState"));
         }
    }
}