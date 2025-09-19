using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;

public interface ITargetingComponent
{
    public Collider TargetingCollider { get; set; }
    public CharacterCore CharacterCore { get; set; }
    public Transform ParentTransform { get; set; }
    public CharacterController CharacterController { get; set; }
    public HashSet<Transform> Targets { get; set; }
    public bool IsAllowed { get; set; }
    public bool IsSelectedCharacter { get; set; }
    
    public Action<Transform> OnTargetAdded { get; set; }
    public Action<Transform> OnTargetRemoved { get; set; }

    public void OnCharacterSelected(CharacterCore characterCoreSelected);
    public void ShowTargetName(Transform target, bool show);
}

public static class TargetingComponentExtensions
{
    
    [BurstCompile]
    public static Transform GetFirstTarget(this ITargetingComponent component)
    {
        return component.Targets.FirstOrDefault();
    }
    
    public static void Allow(this ITargetingComponent component, bool allow)
    {
        if (component.Targets == null)
        {
            return;
        }
    
        component.IsAllowed = allow;

        if (component.IsAllowed)
        {
            return;
        }
        foreach (var target in component.Targets.ToList()) 
        {
            component.RemoveTarget(target);
        }
        component.Targets.Clear(); 
    }
    
    [BurstCompile]
    public static float GetHorizontalAngleToFirstTarget(this ITargetingComponent component)
    {
        var target = component.GetFirstTarget();
        return component.CharacterController.GetHorizontalAngle(target);
    }
    
    [BurstCompile]
    public static float GetVerticalAngleToFirstTarget(this ITargetingComponent component)
    {
        var target = component.GetFirstTarget();
        return component.CharacterController.GetVerticalAngle(target);
    }
    
    [BurstCompile]
    private static void AddTarget(this ITargetingComponent component, Transform target)
    {
        component.Targets.Add(target);
        component.OnTargetAdded?.Invoke(target);

        if (component.IsSelectedCharacter)
        {
            component.ShowTargetName(target,true);
        }
    }

    [BurstCompile]
    public static void RemoveTarget(this ITargetingComponent component, Transform target)
    {
        component.Targets.Remove(target);
        component.OnTargetRemoved?.Invoke(target);

        if (component.IsSelectedCharacter)
        {
            component.ShowTargetName(target,false);
        }
    }
    
    [BurstCompile]
    public static void Check(this ITargetingComponent component, Collider other)
    {
        if (!component.IsAllowed)
        {
            return;
        }
        
        if (other.gameObject.transform == component.ParentTransform)
        {
            return;
        }

        if (component.Targets.Contains(other.transform))
        {
            return;
        }
        component.AddTarget(other.transform);
    }
}