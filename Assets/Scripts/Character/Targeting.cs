using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Targeting : MonoBehaviour
{
    [field: SerializeField] private Collider targetingCollider;
    [field: SerializeField] private Transform parent;
    private HashSet<Transform> _targets;
    private bool _allowed;
    
    private void Awake()
    {
        _targets = new HashSet<Transform>();
    }

    public Transform GetFirstTarget()
    {
        return _targets.FirstOrDefault();
    }

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
    
    private void AddTarget(Transform target)
    {
        _targets.Add(target);
        Debug.LogWarning($"Adding {target.name}");
    }

    private void RemoveTarget(Transform target)
    {
        if (!_targets.Contains(target))
        {
            return;
        }
        _targets.Remove(target);
        Debug.LogWarning($"Removing {target.name}");
    }

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
