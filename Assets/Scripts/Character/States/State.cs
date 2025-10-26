using UnityEngine;

public abstract class State : ScriptableObject
{
    [field: Header("Clips")]
    [field: SerializeField] public float EnterTransitionDuration {get; private set;}
    [field: SerializeField] public bool ApplyRootMotion {get; private set;}
    [field: SerializeField] public  AnimationBlendConfig[]  Clips { get; set; }
    
    [field: Header("Targeting")]
    [field: SerializeField] protected bool AllowItemTargeting { get; set; }
    [field: SerializeField] protected bool AllowCharacterTargeting { get; set; }
    [field: SerializeField] protected bool FixOnCharacterTarget {get; private set;}
    [field: SerializeField] protected bool FixOnItemTarget {get; private set;}
    [field: SerializeField, Range(0f, 1f)] public float TargetingRigWeight {get; private set;} 
    
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
    [field: SerializeField] protected bool SetCapsuleToTrigger { get; private set; }
    
    [field: Header("Inventory")]
    [field: SerializeField] public bool UseInventory {get; private set;}
   
    public virtual void EnterState(CharacterCore character)
    {
        character.OnStateChanged?.Invoke();
        
        character.TargetingSystem.AllowItemTargeting(AllowItemTargeting);
        character.TargetingSystem.AllowCharacterTargeting(AllowCharacterTargeting);
        
        character.CharacterColliderSizer.SetSize(Height, Radius);
        
        character.StateTimer.ResetTime();
        character.StateTimer.SetActionIsPossible(true);
    }

    public virtual void UpdateState(CharacterCore character)
    {
        if (RotationByCamera)
        {
            character.UpdateRotation(character.SceneCamera.SceneCameraData.MainCamera.eulerAngles, RotationSpeed);
        }

        if (FixOnCharacterTarget && character.TargetingSystem.LastCharacterTransform)
        {
           // реализовать
        }
        
        if (FixOnItemTarget && character.TargetingSystem.LastItemTransform)
        {
            // реализовать
        }
        
        character.StateTimer.OnUpdate(Time.deltaTime);
        
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
        character.CharacterInputHandler.ResetAttack(); 
        character.CharacterColliderSizer.SetEnabled(SetCapsuleToTrigger);
        
        character.TargetingSystem.AllowItemTargeting(false);
        character.TargetingSystem.AllowCharacterTargeting(false);
    }
}
