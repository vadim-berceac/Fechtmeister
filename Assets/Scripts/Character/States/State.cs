using UnityEngine;

public abstract class State : ScriptableObject
{
    [field: Header("Animation")]
    [field: SerializeField] protected float EnterTransitionDuration {get; private set;}
    [field: SerializeField] protected int AnimationLayer {get; private set;}
    [field: SerializeField] protected bool ApplyRootMotion {get; private set;}
    
    [field: Header("Locomotion")]
    [field: SerializeField] protected bool RotationByCamera {get; private set;}
    [field: SerializeField] protected float RotationSpeed {get; private set;}
    
    [field: Header("Gravity")]
    [field: SerializeField] protected bool UseGravity {get; private set;}
    [field: SerializeField] protected LayerMask GroundLayer {get; private set;}

    public virtual void EnterState(CharacterCore character)
    {
        character.LocomotionSettings.Animator.applyRootMotion = ApplyRootMotion;
    }

    public virtual void UpdateState(CharacterCore character)
    {
        character.UpdateRotationByCamera(RotationByCamera, RotationSpeed);
    }

    public virtual void FixedUpdateState(CharacterCore character)
    {
        character.Gravity.SetFallSpeed(character.ApplyGravitation(UseGravity, character.Gravity.CurrentFallSpeed, true));
        character.Gravity.SetGrounded(character.CheckIsGrounded(UseGravity, GroundLayer));
        character.UpdateFallDetection(UseGravity);
    }
    
    public abstract void CheckSwitch(CharacterCore character);
    public abstract void ExitState(CharacterCore character);
   
}
