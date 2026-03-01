using Unity.Burst;
using UnityEngine;

public static class CharacterCoreExtensions
{
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
        character.CashedTransform.rotation = Quaternion.Slerp(character.CashedTransform.rotation, 
            targetRotation, Time.deltaTime * rotationSpeed);
    }
 
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
                character.Health.Damage(fallDistance * fallDistance, DamageTypes.Environment);
            }

            character.Gravity.SetMaxHeightReached(currentHeight);
        }

        character.Gravity.SetWasGroundedLastFrame(character.Gravity.Grounded);
    }
  
    public static void MoveLocal(this CharacterCore character, Vector3 direction, float speed)
    {
        var moveDirection = character.CashedTransform.TransformDirection(direction);
        character.CharacterController.Move(Time.deltaTime * speed * moveDirection);
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
    
     public static void MoveToLedgeBlended(this CharacterCore character, float speed,
         float pullTowardsWallFactor = 0.5f, float pullIntensity = 3f,  float stopDistance = 0.05f,
         bool useSnap = true, float ledgeDepth = 0.8f, float handToFeetOffset = 0.05f)
     {
         var currentPosition = character.CashedTransform.position;
         var grabPoint = character.LedgeDetection.LastLedgeGrabPoint;
    
        if (grabPoint == Vector3.zero)
        {
            return;
        }
    
        var wallNormal = GetWallNormal(character, currentPosition, grabPoint);

        var inwardDir = GetInwardDirection(wallNormal);
        var ledgeSurfaceY = grabPoint.y + handToFeetOffset; 
        var ledgeTargetPosition = GetLedgeTargetPosition(grabPoint, inwardDir, ledgeDepth, ledgeSurfaceY);

        var delta = ledgeTargetPosition - currentPosition;
        var heightDiff = Mathf.Abs(delta.y);
        var horizDist = new Vector2(delta.x, delta.z).magnitude;

        if (IsHeightAligned(currentPosition.y, ledgeSurfaceY, stopDistance))
        {
            MoveToSurface(character, currentPosition, ledgeTargetPosition, wallNormal);
            return;
        }

        const float snapThreshold = 0.2f;
        if (useSnap && ShouldSnap(horizDist, heightDiff, snapThreshold))
        {
            SnapToLedge(character, currentPosition, ledgeTargetPosition, ledgeSurfaceY, horizDist, ledgeDepth, wallNormal);
            return;
        }
       
        var reachedLledgeHeight = Mathf.Abs(currentPosition.y - ledgeSurfaceY) < 0; 
        var movementDir = GetSwitchedDirection(character, currentPosition, ledgeTargetPosition, ledgeSurfaceY,
            reachedLledgeHeight, pullTowardsWallFactor, pullIntensity);
    
        if (reachedLledgeHeight)
        {
            var pos = character.CashedTransform.position;
            character.CashedTransform.position = new Vector3(pos.x, ledgeSurfaceY, pos.z);
        }
        character.MoveLocal(movementDir, speed);
     }

     private static Vector3 GetWallNormal(CharacterCore character, Vector3 currentPosition, Vector3 grabPoint)
     {
         var wallNormal = character.LedgeDetection.LastWallNormal;
         if (wallNormal == Vector3.zero)
         {
             var dirToGrab = (grabPoint - currentPosition).normalized;
             var distToGrab = Vector3.Distance(currentPosition, grabPoint);
             if (Physics.Raycast(currentPosition, dirToGrab, out var hit, distToGrab, character.LedgeDetectionSettings.LayerMask))
             {
                 wallNormal = hit.normal;
                 wallNormal.y = 0f; 
                 wallNormal = wallNormal.normalized;
             }
             else
             {
                 wallNormal = (currentPosition - grabPoint).normalized;
                 wallNormal.y = 0f;
                 wallNormal = wallNormal.normalized;
             }
         }
         return wallNormal;
     }

     private static Vector3 GetInwardDirection(Vector3 wallNormal)
     {
         var inwardDir = -wallNormal.normalized; 
         inwardDir.y = 0f;
         return inwardDir;
    
     }

     private static Vector3 GetLedgeTargetPosition(Vector3 grabPoint, Vector3 inwardDir, float depth, float surfaceY)
     {
         var target = grabPoint + inwardDir * depth;
         target.y = surfaceY;
         return target;
     }

     private static bool IsHeightAligned(float currentY, float targetY, float threshold)
     {
         return Mathf.Abs(currentY - targetY) <= threshold;
     }

     private static void MoveToSurface(CharacterCore character, Vector3 currentPosition, Vector3 targetPosition, Vector3 wallNormal)
     {
         var smoothTarget = Vector3.Lerp(currentPosition, targetPosition, 0.2f);
         var moveDelta = smoothTarget - currentPosition;
         character.CharacterController.Move(moveDelta);
         character.FaceWallNormal(wallNormal);
     }

     private static bool ShouldSnap(float horizDist, float heightDiff, float threshold)
     {
         return horizDist > threshold && heightDiff < 0.1f;
     }

    private static void SnapToLedge(CharacterCore character, Vector3 currentPosition, Vector3 targetPosition,
    float surfaceY, float horizDist, float depth, Vector3 wallNormal)
    {
        var snapTarget = targetPosition;
        var start = new Vector3(currentPosition.x, surfaceY, currentPosition.z);
        var dir = (targetPosition - start).normalized;
        if (Physics.Raycast(start, dir, out var snapHit, horizDist, character.LedgeDetectionSettings.LayerMask))
        {
            if (Vector3.Angle(snapHit.normal, Vector3.up) < 45f)
            {
                snapTarget = snapHit.point + snapHit.normal * 0.01f;
            }
            else
            {
                snapTarget = targetPosition;
            }
            snapTarget.y = surfaceY;
        }
        else
        {
            snapTarget = targetPosition;
        }

        character.CashedTransform.position = snapTarget;
        character.CharacterController.Move(Vector3.zero);
        character.FaceWallNormal(wallNormal);
    }

    private static Vector3 GetSwitchedDirection(CharacterCore character, Vector3 currentPosition, Vector3 targetPosition,
        float surfaceY, bool reachedLedgeHeight, float pullFactor, float pullIntensity)
    {
        if (reachedLedgeHeight)
        {
            var actualCurrentPos = character.CashedTransform.position; 
            var localTargetPos = character.CashedTransform.InverseTransformPoint(targetPosition);
            var localCurrentPos = character.CashedTransform.InverseTransformPoint(actualCurrentPos);
            var localHorizDir = (localTargetPos - localCurrentPos);
            localHorizDir.y = 0f;
            return localHorizDir.normalized;
        }
        
        var worldHorizDir = (targetPosition - currentPosition).normalized;
        var verticalDir = character.CashedTransform.up * Mathf.Sign(surfaceY - currentPosition.y);

        var horizontalDir = character.CashedTransform.InverseTransformDirection(worldHorizDir);
        horizontalDir.y = 0f;

        var strongPullDir = pullFactor * pullIntensity * horizontalDir;
        return (verticalDir + strongPullDir).normalized;
    }
    
    public static void Take(this CharacterCore character)
    {
        var item = character.TargetingSystem.GetTargetItem();
        if (item.data == null || item.amount <= 0)
        {
            return;
        }
        
        character.Inventory.AddToInventoryBag(item.data, item.amount);
    }
    
    public static void SetAnimationByWeaponIndex(this CharacterCore character, State inState)
    {
        var animIndex = character.CharacterInputHandler.IsWeaponDraw
            ? ((WeaponData)character.Inventory.WeaponSystem.InstanceInHands.EquppiedItemData).AnimationType
            : 0;
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(inState, animIndex);
    }
    
    public static void CastAttack(this CharacterCore character, MovementState inState)
    {
        if (!inState.FastAttackAllowed)
        {
            return;
        }
        if (!character.CharacterInputHandler.IsWeaponDraw)
        {
            return;
        }
        if (character.CharacterInputHandler.IsAttack && !character.Inventory.WeaponSystem.WeaponInstanceIsRanged 
                                                     && character.GraphCore.UpperBodyLayerController.IsComplete())
        {
            character.SetSubState(character.StatesSet.GetState("FastAttackSubState"));
        } 
    }
    
    public static void Move(this CharacterCore character, float inputX, float inputY)
    {
        character.GraphCore.FullBodyAnimatorController.Move(inputX, inputY);
    }
}