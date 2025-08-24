using Unity.Burst;
using UnityEngine;

public static class CharacterCoreExtensions
{
    [BurstCompile]
    public static void UpdateRotationByCamera(this CharacterCore character, bool rotationByCamera, float rotationSpeed)
    {
        if (!rotationByCamera)
        {
            return;
        }
        if (character.SceneCamera.Target != character.CashedTransform)
        {
            return;
        }
        
        var targetRotation = Quaternion.Euler(0, character.SceneCamera.SceneCameraData.MainCamera.eulerAngles.y, 0);
        
        if (Quaternion.Angle(character.CashedTransform.rotation, targetRotation) < 0.1f)
        {
            character.CashedTransform.rotation = targetRotation;
            return;
        }
        character.CashedTransform.rotation = Quaternion.Slerp(character.CashedTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
    
    [BurstCompile]
    public static float ApplyGravitation(this CharacterCore character, bool useGravity, float currentFallSpeed, bool isOnValidGround)
    {
        if (!useGravity)
        {
            return 0;
        }
        
        if (isOnValidGround)
        {
            if (currentFallSpeed < 0f)
            {
                currentFallSpeed = -2f;
            }
        }
        else
        {
            var gravityDelta = GravitationConstants.GravitationForce * Time.fixedDeltaTime;
            currentFallSpeed = Mathf.Max(currentFallSpeed - gravityDelta, - GravitationConstants.MaxFallSpeed);
        }

        var moveVector = currentFallSpeed * Time.fixedDeltaTime * Vector3.up;
        var previousPosition = character.LocomotionSettings.CharacterController.transform.position;
        character.LocomotionSettings.CharacterController.Move(moveVector);
        var newPosition = character.CashedTransform.position;

        if (!isOnValidGround && moveVector.y < 0f && Mathf.Approximately(newPosition.y, previousPosition.y))
        {
            currentFallSpeed = -2f;
        }
        
        return currentFallSpeed;
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
            //_landingSfx.PlayRandomAtPoint(CashedTransform.position);
            Debug.LogWarning("Проигрываем звук приземления");
            var fallDistance = character.Gravity.MaxHeightReached - currentHeight;
            if (fallDistance > character.GravitySettings.FallDamageThreshold && !character.GravitySettings.ImmuneToFallDamage)
            {
                //OnFallDamage?.Invoke(fallDistance);
                //Debug.LogWarning("Наносим урон от падения");
                character.CharacterInputHandler.FallDamageReaction(true);
            }

            character.Gravity.SetMaxHeightReached(currentHeight);
        }

        character.Gravity.SetWasGroundedLastFrame(character.Gravity.Grounded);
    }
}
