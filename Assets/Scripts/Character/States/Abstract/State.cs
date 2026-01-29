using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class State : ScriptableObject
{
    [field: Header("Clips")]
    [field: SerializeField] public float EnterTransitionDuration {get; private set;}
    [field: SerializeField] public bool ApplyRootMotion {get; private set;}
    [field: SerializeField] public  AnimationBlendConfig[]  Clips { get; set; }
    
    [field: Header("Targeting")]
    [field: SerializeField] protected bool AllowItemTargeting { get; set; }
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
   
    
    [field: Header("Inventory")]
    [field: SerializeField] public bool UseInventory {get; private set;}
    
    protected  List<Transition<CharacterCore>> Transitions { get;  set; }
   
    public virtual void EnterState(CharacterCore character)
    {
        character.OnStateChanged?.Invoke();
        
        character.TargetingSystem.AllowItemTargeting(AllowItemTargeting);
        
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
        
        if (FixOnItemTarget && character.TargetingSystem.LastItemTransform)
        {
            // реализовать
        }
        
        character.StateTimer.OnUpdate(Time.deltaTime);
        
        CheckSwitch(character);
    }

    public virtual void FixedUpdateState(CharacterCore character)
    {
        character.Gravity.SetGrounded(character.CheckIsGrounded(UseGravity, GroundLayer));
        character.UpdateFallDetection(UseGravity);
    }

    public virtual void LateUpdateState(CharacterCore character)
    {
        CheckAction(character);
    }

    protected virtual void CheckSwitch(CharacterCore character)
    {
        if (Transitions == null || Transitions.Count == 0)
        {
            return;
        }
        foreach (var t in Transitions)
        {
            if (t.Check(character))
            {
                character.SetState(character.StatesSet.GetState(t.TargetStateName));
            }
        }
    }

    protected virtual void CheckAction(CharacterCore character)
    {
        
    }

    public virtual void ExitState(CharacterCore character)
    {
        character.CharacterInputHandler.ResetInputBuffer();
        character.TargetingSystem.AllowItemTargeting(false);
        character.CharacterColliderSizer.SetSize(SizeMode.Full, SizeMode.Full);
    }
}
