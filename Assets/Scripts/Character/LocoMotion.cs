
using UnityEngine;

public class LocoMotion : IInputHandler
{
    public IInputSet InputSet { get; private set; }
    
    private readonly LocoMotionSettings _locoMotionSettings;
    private ICharacterInputSet _characterInputSet;

    public LocoMotion(LocoMotionSettings locoMotionSettings)
    {
        _locoMotionSettings = locoMotionSettings;
    }

    public void SetupInputSet(IInputSet inputSet)
    {
        if (inputSet is ICharacterInputSet)
        {
            _characterInputSet = (ICharacterInputSet)inputSet;
            Subscribe();
            return;
        }

        if (inputSet == null)
        {
            Unsubscribe();
            _characterInputSet = null;
            return;
        }
        Debug.LogWarning("LocoMotion wrong input set");
    }

    private void Subscribe()
    {
        if (_characterInputSet == null)
        {
            return;
        }
        _characterInputSet.OnMove += OnMove;
        _characterInputSet.OnMove += RotateCharacter;
    }

    private void Unsubscribe()
    {
        if (_characterInputSet == null)
        {
            return;
        }
        _characterInputSet.OnMove -= OnMove;
        _characterInputSet.OnMove -= RotateCharacter;
    }

    private void OnMove(Vector2 move)
    {
        _locoMotionSettings.Animator.SetFloat(_locoMotionSettings.InputX, move.x);
        _locoMotionSettings.Animator.SetFloat(_locoMotionSettings.InputY, move.y);
    }
    
    private void RotateCharacter(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f) return;
        Vector3 inputDirection = new Vector3(input.x, 0, input.y).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
        Quaternion newRotation = Quaternion.Slerp(_locoMotionSettings.Rigidbody.rotation, targetRotation, _locoMotionSettings.RotationSpeed * Time.fixedDeltaTime);
        _locoMotionSettings.Rigidbody.MoveRotation(newRotation);
        Debug.Log($"Applied Rotation: {_locoMotionSettings.Rigidbody.rotation}");
    }
}

[System.Serializable]
public struct LocoMotionSettings
{
    [field: Header("Animator")]
    [field: SerializeField] public Animator Animator { get; private set; }
    
    [field: Header("Rigidbody")]
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
    
    [field: Header("Animation Params")]
    [field: SerializeField] public float RotationSpeed { get; private set; }
    [field: SerializeField] public float MaxSlopeAngle { get; private set; }
    [field: SerializeField] public string InputY { get; private set; }
    [field: SerializeField] public string InputX { get; private set; }
}
