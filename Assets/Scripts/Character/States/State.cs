using System.Linq;
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
    [field: SerializeField] protected bool SpineRotationCorrection {get; private set;}
    
    [field: Header("Gravity")]
    [field: SerializeField] protected bool UseGravity {get; private set;}
    [field: SerializeField] protected LayerMask GroundLayer {get; private set;}
    
    [field: Header("Layers")]
    [field: SerializeField] protected AdditionalLayer[] AdditionalLayers {get; private set;}
    
    public virtual void EnterState(CharacterCore character)
    {
        character.LocomotionSettings.Animator.applyRootMotion = ApplyRootMotion;
        
        if (SpineRotationCorrection)
        {
            character.LocomotionSettings.SpineProxy.Allow(true);
        }
        
        this.CorrectLayersWeight(character, AdditionalLayers, EnterTransitionDuration);
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

    public virtual void ExitState(CharacterCore character)
    {
        if (SpineRotationCorrection)
        {
            character.LocomotionSettings.SpineProxy.Allow(false);
        }
    }
}

[System.Serializable]
public struct AdditionalLayer
{
    [field: SerializeField] public int Layer { get; private set; }
    [field: SerializeField] public float Weight { get; private set; }
    [field: SerializeField] public int[] ExcludedWeaponIndices { get; private set; }
}
