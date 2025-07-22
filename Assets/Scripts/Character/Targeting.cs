using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;

public class Targeting : MonoBehaviour
{
    [field: SerializeField] private Collider targetingCollider;
    [field: SerializeField] private Transform parent;
    [field: SerializeField] private CharacterController characterController;
    private HashSet<Transform> _targets;
    private bool _allowed;
    
    private void Awake()
    {
        _targets = new HashSet<Transform>();
    }

    [BurstCompile]
    public Transform GetFirstTarget()
    {
        return _targets.FirstOrDefault();
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
        Debug.LogWarning($"Adding {target.name}");
    }

    [BurstCompile]
    private void RemoveTarget(Transform target)
    {
        if (!_targets.Contains(target))
        {
            return;
        }
        _targets.Remove(target);
        Debug.LogWarning($"Removing {target.name}");
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
}
