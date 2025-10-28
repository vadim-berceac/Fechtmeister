using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "LandingState", menuName = "States/LandingState")]
public class LandingState: State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        var param = character.CurrentSpeed.LastNotNullHorizontalSpeed > 4 ? 1 : 0;
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, param);
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
         
         if (!character.CharacterInputHandler.IsWeaponDraw && character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.75
                                                           && character.CurrentSpeed.LastNotNullHorizontalSpeed > 4 
                                                           && character.CharacterInputHandler.IsRun)
         {
             character.SetState(character.StatesContainer.GetState("RunState"));
         }
         
         if (character.CharacterInputHandler.IsWeaponDraw && character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.75
                                                           && character.CurrentSpeed.LastNotNullHorizontalSpeed > 4 
                                                           && character.CharacterInputHandler.IsRun)
         {
             character.SetState(character.StatesContainer.GetState("CombatRunState"));
         }
         
         if (character.Health.IsDestroyed)
         {
             character.SetState(character.StatesContainer.GetState("DeathState"));
         }
    }
}