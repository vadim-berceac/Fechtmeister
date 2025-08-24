using Unity.Burst;
using UnityEngine;

public abstract class State : ScriptableObject
{
    [field: Header("Targeting")]
    [field: SerializeField] protected bool AllowItemTargeting { get; set; }
    [field: SerializeField] protected bool AllowCharacterTargeting { get; set; }
    [field: SerializeField] protected bool UpdateHorizontalTargetAngle { get; set; }
    [field: SerializeField] protected bool UpdateVerticalTargetAngle { get; set; }
    
    [field: Header("Input")]
    [field: SerializeField] public bool AllowSwitchWeaponInstance { get; set; }
    
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
    
    [field: Header("Inventory")]
    [field: SerializeField] public bool UseInventory {get; private set;}
    
    public virtual void EnterState(CharacterCore character)
    {
        character.LocomotionSettings.Animator.applyRootMotion = ApplyRootMotion;
        
        if (SpineRotationCorrection)
        {
            character.LocomotionSettings.SpineProxy.Allow(true);
        }
        
        character.TargetingSystem.AllowItemTargeting(AllowItemTargeting);
        character.TargetingSystem.AllowCharacterTargeting(AllowCharacterTargeting);

        if (!UpdateVerticalTargetAngle)
        {
            character.LocomotionSettings.Animator.SetFloat(AnimationParams.VerticalAngleToTarget, 0);
        }
        
        if (!UpdateHorizontalTargetAngle)
        {
            character.LocomotionSettings.Animator.SetFloat(AnimationParams.HorizontalAngleToTarget, 0);
        }
        
        this.CorrectLayersWeight(character, AdditionalLayers, EnterTransitionDuration);
    }

    public virtual void UpdateState(CharacterCore character)
    {
        character.UpdateRotationByCamera(RotationByCamera, RotationSpeed);

        UpdateTargetingParams(character);
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
        
        character.CharacterInputHandler.ResetAttack(); // чтобы атаки не накапливались
        
        character.TargetingSystem.AllowItemTargeting(false);
        character.TargetingSystem.AllowCharacterTargeting(false);
    }

    [BurstCompile]
    private void UpdateTargetingParams(CharacterCore character)
    {
        if (AllowCharacterTargeting)
        {
            if (UpdateVerticalTargetAngle)
            {
                character.LocomotionSettings.Animator.SetFloat(AnimationParams.VerticalAngleToTarget,  character.TargetingSystem.GetVerticalAngle(TargetingMode.Character));
            }
        
            if (UpdateHorizontalTargetAngle)
            {
                character.LocomotionSettings.Animator.SetFloat(AnimationParams.HorizontalAngleToTarget, character.TargetingSystem.GetHorizontalAngle(TargetingMode.Character));
            }
            return;
        }

        if (!AllowItemTargeting)
        {
            return;
        }
        if (UpdateVerticalTargetAngle)
        {
            character.LocomotionSettings.Animator.SetFloat(AnimationParams.VerticalAngleToTarget,  character.TargetingSystem.GetVerticalAngle(TargetingMode.Item));
        }
        
        if (UpdateHorizontalTargetAngle)
        {
            character.LocomotionSettings.Animator.SetFloat(AnimationParams.HorizontalAngleToTarget, character.TargetingSystem.GetHorizontalAngle(TargetingMode.Item));
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
