using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;

public class Targeting : MonoBehaviour
{
    [field: SerializeField] private Collider targetingCollider;
    [field: SerializeField] private Transform parent;
    [field: SerializeField] private CharacterController characterController;
    [field: SerializeField] private CharacterCore characterCore;
    private HashSet<Transform> _targets;
    private bool _allowed;
    private bool _selectedCharacter;
    public Action<Transform> OnTargetAdded;
    public Action<Transform> OnTargetRemoved;
    
    private void Awake()
    {
        _targets = new HashSet<Transform>();
        CharacterSelector.OnCharacterSelected += OnCharacterSelected;
    }

    [BurstCompile]
    public Transform GetFirstTarget()
    {
        return _targets.FirstOrDefault();
    }

    private void OnCharacterSelected(CharacterCore characterCoreSelected)
    {
        if (characterCoreSelected == characterCore)
        {
            _selectedCharacter = true;
            return;
        }
        _selectedCharacter = false;
        foreach (var target in _targets)
        {
            target.TryGetComponent<PickupItem>(out var pickupItem);

            if (pickupItem != null)
            {
                pickupItem.ShowNamePlate(false);
            }
        }
        _targets.Clear();
    }

    [BurstCompile]
    public void Allow(bool allow)
    {
        if (_targets == null)
        {
            return;
        }
        
        _allowed = allow;

        if (!_allowed)
        {
            _targets.Clear();
        }
    }
    
    [BurstCompile]
    public float GetHorizontalAngleToFirstTarget()
    {
        var target = GetFirstTarget();
        return characterController.GetHorizontalAngle(target);
    }
    
    [BurstCompile]
    public float GetVerticalAngleToFirstTarget()
    {
        var target = GetFirstTarget();
        return characterController.GetVerticalAngle(target);
    }
    
    [BurstCompile]
    private void AddTarget(Transform target)
    {
        _targets.Add(target);
        OnTargetAdded?.Invoke(target);

        if (_selectedCharacter)
        {
            ShowTargetName(target,true);
        }
    }

    [BurstCompile]
    public void RemoveTarget(Transform target)
    {
        _targets.Remove(target);
        OnTargetRemoved?.Invoke(target);

        if (_selectedCharacter)
        {
            ShowTargetName(target,false);
        }
    }

    [BurstCompile]
    private void Check(Collider other)
    {
        if (!_allowed)
        {
            return;
        }
        
        if (other.gameObject.transform == parent)
        {
            return;
        }

        if (_targets.Contains(other.transform))
        {
            return;
        }
        AddTarget(other.transform);
    }

    private static void ShowTargetName(Transform target, bool show)
    {
        target.TryGetComponent<PickupItem>(out var item);

        if (item == null)
        {
            return;
        }
        
        item.ShowNamePlate(show);
    }
    
    private void OnTriggerEnter(Collider other)
    {
       Check(other);
    }

    private void OnTriggerStay(Collider other)
    {
        Check(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_allowed)
        {
            return;
        }
        
        RemoveTarget(other.transform);
    }

    private void OnDisable()
    {
        CharacterSelector.OnCharacterSelected -= OnCharacterSelected;
    }
}
