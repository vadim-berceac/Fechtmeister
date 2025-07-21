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
    
    [BurstCompile]
    public float GetHorizontalAngleToFirstTarget()
    {
        var target = GetFirstTarget();
        if (target == null || parent == null)
        {
            return 0f;
        }

        var parentCenter = characterController.bounds.center;
        var targetPosition = target.position;
        var directionToTarget = new Vector3(targetPosition.x - parentCenter.x, 0f, targetPosition.z - parentCenter.z).normalized;
        var parentForward = new Vector3(parent.forward.x, 0f, parent.forward.z).normalized;
        
        var angle = Vector3.SignedAngle(parentForward, directionToTarget, Vector3.up);
        return angle;
    }
    
    [BurstCompile]
    public float GetVerticalAngleToFirstTarget()
    {
        var target = GetFirstTarget();
        if (target == null || parent == null)
        {
            return 0f;
        }
        var parentCenter = characterController.bounds.center;
        var targetPosition = target.position;
        var directionToTarget = (targetPosition - parentCenter).normalized;
        var parentForward = parent.forward.normalized;
        var parentRight = parent.right.normalized;
        var forwardInPlane = Vector3.ProjectOnPlane(parentForward, parentRight).normalized;
        var directionInPlane = Vector3.ProjectOnPlane(directionToTarget, parentRight).normalized;
        var angle = Vector3.SignedAngle(forwardInPlane, directionInPlane, -parentRight);
        return angle;
    }
}
