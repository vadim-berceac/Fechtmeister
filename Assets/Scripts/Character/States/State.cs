using UnityEngine;

public abstract class State : ScriptableObject
{
    [field: Header("Animation")]
    [field: SerializeField] protected float EnterTransitionDuration {get; private set;}
    [field: SerializeField] protected int AnimationLayer {get; private set;}
    
    [field: Header("Locomotion")]
    [field: SerializeField] protected bool RotationByCamera {get; private set;}
    [field: SerializeField] protected float RotationSpeed {get; private set;}
    
    [field: Header("Gravity")]
    [field: SerializeField] protected bool UseGravity {get; private set;}
    [field: SerializeField] protected LayerMask GroundLayer {get; private set;}
    
    public abstract void EnterState(CharacterCore character);

    public virtual void UpdateState(CharacterCore character)
    {
        character.UpdateRotationByCamera(RotationByCamera, RotationSpeed);
    }

    public virtual void FixedUpdateState(CharacterCore character)
    {
        character.SetFallSpeed(character.ApplyGravitation(UseGravity, character.CurrentFallSpeed, true));
        character.SetGrounded(character.CheckIsGrounded(UseGravity, GroundLayer));
    }
    
    public abstract void CheckSwitch(CharacterCore character);
    public abstract void ExitState(CharacterCore character);
   
}
