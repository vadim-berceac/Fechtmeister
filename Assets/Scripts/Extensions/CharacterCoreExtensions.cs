using Unity.Burst;
using UnityEngine;

public static class CharacterCoreExtensions
{
    [BurstCompile]
    public static void UpdateRotation(this CharacterCore character, Vector3 rotation, float rotationSpeed)
    {
        if (character.SceneCamera.Target != character.CashedTransform)
        {
            return;
        }
        var targetRotation = Quaternion.Euler(0, rotation.y, 0);
        
        if (Quaternion.Angle(character.CashedTransform.rotation, targetRotation) < 0.1f)
        {
            character.CashedTransform.rotation = targetRotation;
            return;
        }
        character.CashedTransform.rotation = Quaternion.Slerp(character.CashedTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
    
    [BurstCompile]
    public static bool CheckIsGrounded(this CharacterCore character, bool useGravity, int layerMask)
    {
        if (!useGravity)
        {
            return false;
        }
        
        var spherePos = character.CashedTransform.position + character.GravitySettings.GroundOffset;
        var hitColliders = new Collider[32]; 
        var hitsCount = Physics.OverlapSphereNonAlloc(spherePos, character.GravitySettings.CheckSphereRadius, hitColliders, layerMask);
        return hitsCount > 0;
    }
    
    [BurstCompile]
    public static void UpdateFallDetection(this CharacterCore character,  bool useGravity)
    {
        if (!useGravity)
        {
            return;
        }
        
        var currentHeight = character.CashedTransform.position.y;
        
        if (!character.Gravity.Grounded)
        {
            character.Gravity.SetMaxHeightReached(Mathf.Max(character.Gravity.MaxHeightReached, currentHeight));
        }
        
        if (character.Gravity.Grounded && !character.Gravity.WasGroundedLastFrame)
        {
            var fallDistance = character.Gravity.MaxHeightReached - currentHeight;
            if (fallDistance > character.GravitySettings.FallDamageThreshold && !character.GravitySettings.ImmuneToFallDamage)
            {
                character.Health.Damage(fallDistance * fallDistance);
            }

            character.Gravity.SetMaxHeightReached(currentHeight);
        }

        character.Gravity.SetWasGroundedLastFrame(character.Gravity.Grounded);
    }

    [BurstCompile]
    public static void MoveLocal(this CharacterCore character, Vector3 direction, float speed)
    {
        var moveDirection = character.CashedTransform.TransformDirection(direction);
        character.LocomotionSettings.CharacterController.Move(Time.deltaTime * speed * moveDirection);
    }
    
    [BurstCompile]
    public static void MoveToLedge(this CharacterCore character, float speed, float stopDistance = 0.05f)
    {
        var currentPosition = character.CashedTransform.position;
        var targetPosition = character.LedgeDetection.LastLedgeGrabPoint;
        
        if ((currentPosition - targetPosition).magnitude <= stopDistance)
        {
            character.LocomotionSettings.CharacterController.Move(Vector3.zero);
            return;
        }
        
        var worldDirection = (targetPosition - currentPosition).normalized;

        var localDirection = character.CashedTransform.InverseTransformDirection(worldDirection);

        character.MoveLocal(localDirection, speed);
    }
    
    [BurstCompile]
    public static void FaceWallNormal(this CharacterCore character, Vector3 wallNormal)
    {
        var lookDirection = -wallNormal;
        lookDirection.y = 0f; 

        if (lookDirection != Vector3.zero)
        {
            character.CashedTransform.forward = lookDirection.normalized;
        }
    }
}
