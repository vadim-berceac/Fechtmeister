using Unity.Burst;
using UnityEngine;

public static class CharacterControllerExtensions
{
    public static float GetHorizontalAngle(this CharacterController characterController, Transform target)
    {
        if (target == null)
        {
            return 0f;
        }

        var parentCenter = characterController.bounds.center;
        var targetPosition = target.position;
        var directionToTarget = new Vector3(targetPosition.x - parentCenter.x, 0f, targetPosition.z - parentCenter.z).normalized;
        var parentForward = new Vector3(characterController.transform.forward.x, 0f, characterController.transform.forward.z).normalized;
        
        var angle = Vector3.SignedAngle(parentForward, directionToTarget, Vector3.up);
        return angle;
    }
    
    public static float GetVerticalAngle(this CharacterController characterController, Transform target)
    {
        if (target == null)
        {
            return 0f;
        }
        var parentCenter = characterController.bounds.center;
        var targetPosition = target.position;
        var directionToTarget = (targetPosition - parentCenter).normalized;
        var parentForward = characterController.transform.forward.normalized;
        var parentRight = characterController.transform.right.normalized;
        var forwardInPlane = Vector3.ProjectOnPlane(parentForward, parentRight).normalized;
        var directionInPlane = Vector3.ProjectOnPlane(directionToTarget, parentRight).normalized;
        var angle = Vector3.SignedAngle(forwardInPlane, directionInPlane, -parentRight);
        return angle;
    }
}
