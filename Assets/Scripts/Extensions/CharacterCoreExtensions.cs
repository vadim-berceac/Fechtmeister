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
  
    public static void MoveLocal(this CharacterCore character, Vector3 direction, float speed)
    {
        var moveDirection = character.CashedTransform.TransformDirection(direction);
        character.LocomotionSettings.CharacterController.Move(Time.deltaTime * speed * moveDirection);
    }
    
    public static void MoveToLedgeBlended(this CharacterCore character, float speed, 
    float blendFactor = 0.5f, float pullTowardsWallFactor = 0.5f, float pullIntensity = 3f, 
    float stopDistance = 0.05f, bool useSnap = true, float ledgeDepth = 0.8f)
    {
        var currentPosition = character.CashedTransform.position;
        var targetPosition = character.LedgeDetection.LastLedgeGrabPoint;
        var wallNormal = character.LedgeDetection.LastWallNormal;
        if (wallNormal == Vector3.zero)
        { 
            wallNormal = (currentPosition - targetPosition).normalized;
            wallNormal.y = 0f;
            wallNormal = -wallNormal.normalized;
            
        }
        var delta = targetPosition - currentPosition;
        var heightDiff = Mathf.Abs(delta.y);
        var horizDist = new Vector2(delta.x, delta.z).magnitude;
        if (horizDist <= stopDistance && heightDiff <= stopDistance)
        {
            character.LocomotionSettings.CharacterController.Move(Vector3.zero);
            return;
            
        }
        const float snapThreshold = 0.2f; 
        if (useSnap && horizDist > snapThreshold && heightDiff < 0.1f)
        {
            var legOffset = 0.8f; 
            var snapTarget = targetPosition - wallNormal * legOffset; 
            snapTarget.y = currentPosition.y;
            if (Physics.Raycast(currentPosition, (snapTarget - currentPosition).normalized,
                    out var snapHit, horizDist, character.LedgeDetectionSettings.LayerMask))
            { 
                snapTarget = snapHit.point + snapHit.normal * character.LocomotionSettings.CharacterController.radius;
            }
            character.CashedTransform.position = snapTarget;
            character.LocomotionSettings.CharacterController.Move(Vector3.zero);
            character.FaceWallNormal(wallNormal);
            return;
        }
        
        var inwardDir = -wallNormal.normalized;
        inwardDir.y = 0f; 
        var deepTarget = targetPosition + inwardDir * ledgeDepth; 
        var worldHorizDir = (deepTarget - currentPosition).normalized; 
        var verticalDir = Vector3.up * Mathf.Sign(delta.y);
        var horizontalDir = character.CashedTransform.InverseTransformDirection(worldHorizDir);
        horizontalDir.y = 0f;
        var deepHorizDist = (deepTarget - new Vector3(currentPosition.x, targetPosition.y, currentPosition.z)).magnitude; 
        var verticalWeight = Mathf.Clamp01(heightDiff / (heightDiff + deepHorizDist));
        var blendedLocalDir = Vector3.Lerp(horizontalDir, verticalDir, verticalWeight * blendFactor);
    
        if (verticalWeight > 0.5f)
        {
            var strongPullDir = pullTowardsWallFactor * pullIntensity * horizontalDir;
            blendedLocalDir = (verticalDir + strongPullDir).normalized;
        }
        else
        {
            blendedLocalDir = horizontalDir.normalized;
        }
    
        character.MoveLocal(blendedLocalDir, speed);
    }
  
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
