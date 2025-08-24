using UnityEngine;

public class CharacterControllerRootMotionSynchronizer : MonoBehaviour
{
    [field: SerializeField] public CharacterControllerRootMotionSynchronizerSettings Settings { get; set; }

    private Transform _character;
    private Vector3 _deltaPosition;
    private float _currentFallSpeed;

    private void Awake()
    {
        _character = transform;
        _currentFallSpeed = 0f;
    }

    private void FixedUpdate()
    {
        if (!Settings.CharacterCore.CurrentState.UseGravity)
        {
            _currentFallSpeed = 0;
        }
        else
        {
            _currentFallSpeed = Settings.CharacterCore.GetCurrentFallSpeed(
                useGravity: true,
                currentFallSpeed: _currentFallSpeed,
                isOnValidGround: Settings.CharacterController.isGrounded
            ) * Settings.CharacterCore.CurrentState.FallSpeedMultiplier;
        }
    }

    private void OnAnimatorMove()
    {
        _deltaPosition = Vector3.zero;
        
        if (Settings.CharacterCore.CurrentState.ApplyRootMotion && Settings.Animator.applyRootMotion)
        {
            _deltaPosition = Settings.Animator.deltaPosition;
            _character.rotation *= Settings.Animator.deltaRotation; 
        }

        _deltaPosition += Vector3.up * _currentFallSpeed * Time.deltaTime;
        
        Settings.CharacterController.Move(_deltaPosition);
    }
}

[System.Serializable]
public struct CharacterControllerRootMotionSynchronizerSettings
{
    [field: SerializeField] public Animator Animator { get; set; }
    [field: SerializeField] public CharacterController CharacterController { get; set; }
    [field: SerializeField] public CharacterCore CharacterCore { get; set; }
}
