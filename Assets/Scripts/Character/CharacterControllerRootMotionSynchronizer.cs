using UnityEngine;

public class CharacterControllerRootMotionSynchronizer : MonoBehaviour
{
    [field: SerializeField] public CharacterControllerRootMotionSynchronizerSettings Settings { get; set; }

    private Transform _character;
    private Transform _characterController;
    private Vector3 _deltaPosition;
    private Quaternion _deltaRotation;
    private float _currentFallSpeed;

    private void Awake()
    {
        _character = transform;
        _characterController = Settings.CharacterController.transform;
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
        if (!Settings.CharacterCore.CurrentState.ApplyRootMotion || !Settings.Animator.applyRootMotion)
            return;
        
        _deltaPosition = Settings.Animator.deltaPosition;
        _deltaRotation = Settings.Animator.deltaRotation;
        
        _character.rotation *= _deltaRotation;
        
        _deltaPosition += Vector3.up * _currentFallSpeed * Time.deltaTime;
       
        Settings.CharacterController.Move(_deltaPosition);
        
        _characterController.rotation = _character.rotation;
    }
}

[System.Serializable]
public struct CharacterControllerRootMotionSynchronizerSettings
{
    [field: SerializeField] public Animator Animator { get; set; }
    [field: SerializeField] public CharacterController CharacterController { get; set; }
    [field: SerializeField] public CharacterCore CharacterCore { get; set; }
}
