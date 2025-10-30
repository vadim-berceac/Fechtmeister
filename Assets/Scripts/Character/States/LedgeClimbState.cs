using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "LedgeClimbState", menuName = "States/LedgeClimbState")]
public class LedgeClimbState: State
{
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, 0);
        character.FaceWallNormal(character.LedgeDetection.LastLedgeNormal);
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
        
        if (character.Health.IsHitReactionEnabled)
        {
            character.SetState(character.StatesContainer.GetState("GetHitState"));
        }
    }

    public override void FixedUpdateState(CharacterCore character)
    {
        base.FixedUpdateState(character);
        
        if (character.GraphCore.FullBodyAnimatorController.GetCurrentClipNormalizedTime() > 0.4f)
        {
            character.MoveToLedge(1.2f);
        }
    }

    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.LedgeDetection.Reset();
    }
}
