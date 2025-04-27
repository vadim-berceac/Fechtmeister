
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
    }

    private void Unsubscribe()
    {
        if (_characterInputSet == null)
        {
            return;
        }
        _characterInputSet.OnMove -= OnMove;
    }

    private void OnMove(Vector2 move)
    {
        Debug.Log($"OnMove: {move} " + _locoMotionSettings.Animator.gameObject.transform.parent.name);
        _locoMotionSettings.Animator.SetFloat(_locoMotionSettings.InputX, move.x);
        _locoMotionSettings.Animator.SetFloat(_locoMotionSettings.InputY, move.y);
    }
}

[System.Serializable]
public struct LocoMotionSettings
{
    [field: Header("Animator")]
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: Header("Animation Params")]
    [field: SerializeField] public string InputY { get; private set; }
    [field: SerializeField] public string InputX { get; private set; }
}
