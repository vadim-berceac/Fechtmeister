using UnityEngine;

public abstract class State : ScriptableObject
{
    [field: Header("Clips")]
    [field: SerializeField] protected float EnterTransitionDuration {get; private set;}
    [field: SerializeField] public bool ApplyRootMotion {get; private set;}
    [field: SerializeField] public  AnimationBlendConfig[]  Clips { get; set; }
    
    [field: Header("Targeting")]
    [field: SerializeField] protected bool AllowItemTargeting { get; set; }
    [field: SerializeField] protected bool AllowCharacterTargeting { get; set; }
    [field: SerializeField] protected bool UpdateHorizontalTargetAngle { get; set; }
    [field: SerializeField] protected bool UpdateVerticalTargetAngle { get; set; }
    
    [field: Header("Input")]
    [field: SerializeField] public bool AllowSwitchWeaponInstance { get; set; }
    
    [field: Header("Locomotion")]
    [field: SerializeField] protected bool RotationByCamera {get; private set;}
    [field: SerializeField] protected float RotationSpeed {get; private set;}
    
    [field: Header("Gravity")]
    [field: SerializeField] public bool UseGravity {get; private set;}

    [field: SerializeField] public float FallSpeedMultiplier { get; private set; } = 1f;
    [field: SerializeField] protected LayerMask GroundLayer {get; private set;}

    [field: Header("Capsule Size")]
    [field: SerializeField] protected SizeMode Height { get; private set; } = SizeMode.Full;
    [field: SerializeField] protected SizeMode Radius { get; private set; } = SizeMode.Full;
    
    [field: Header("Inventory")]
    [field: SerializeField] public bool UseInventory {get; private set;}
   
    public virtual void EnterState(CharacterCore character)
    {
        character.OnStateChanged?.Invoke();
        
        character.LocomotionSettings.Animator.applyRootMotion = ApplyRootMotion;
        
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
        
        character.CharacterColliderSizer.SetSize(Height, Radius);
    }

    public virtual void UpdateState(CharacterCore character)
    {
        character.UpdateRotationByCamera(RotationByCamera, RotationSpeed);

        UpdateTargetingParams(character);
        
        character.PlayablesAnimatorController.OnUpdate();
        
        CheckAction(character);
        
        CheckSwitch(character);
    }

    public virtual void FixedUpdateState(CharacterCore character)
    {
        character.Gravity.SetGrounded(character.CheckIsGrounded(UseGravity, GroundLayer));
        character.UpdateFallDetection(UseGravity);
    }
   
    protected abstract void CheckSwitch(CharacterCore character);

    protected virtual void CheckAction(CharacterCore character)
    {
        
    }

    public virtual void ExitState(CharacterCore character)
    {
        character.CharacterInputHandler.ResetAttack(); // чтобы атаки не накапливались
        
        character.TargetingSystem.AllowItemTargeting(false);
        character.TargetingSystem.AllowCharacterTargeting(false);
    }

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
