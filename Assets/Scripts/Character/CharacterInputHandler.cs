using UnityEngine;

public class CharacterInputHandler : IInputHandler
{
    public IInputSet InputSet { get; private set; }
    private ICharacterInputSet _characterInputSet;

    public bool IsSprint { get; private set; }
    public bool IsWeaponDraw { get; private set; }
    public float InputX { get; private set; }
    public float InputY { get; private set; }
    public float LookX { get; private set; }
    
    private bool _isSubscribed;
    
    public void SetupInputSet(IInputSet inputSet)
    {
        if (inputSet == null)
        {
            Unsubscribe();
            InputSet = null;
            _characterInputSet = null;
            return;
        }
        InputSet = inputSet;
        _characterInputSet = (ICharacterInputSet)InputSet;
        Subscribe();
    }

    private void Subscribe()
    {
        if (_isSubscribed)
        {
            return;
        }
        _isSubscribed = true;
        
        _characterInputSet.OnMove += OnMove;
        _characterInputSet.OnSprint += OnSprint;
        _characterInputSet.OnLook += OnLook;
        _characterInputSet.OnDrawWeapon += OnWeaponDraw;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed)
        {
            return;
        }
        _isSubscribed = false;
        
        _characterInputSet.OnMove -= OnMove;
        _characterInputSet.OnSprint -= OnSprint;
        _characterInputSet.OnLook -= OnLook;
        _characterInputSet.OnDrawWeapon -= OnWeaponDraw;
    }

    private void OnMove(Vector2 move)
    {
        InputX = move.x;
        InputY = move.y;
    }

    private void OnLook(Vector2 look)
    {
        LookX = look.x;
    }

    private void OnSprint()
    {
        IsSprint = !IsSprint;
    }

    private void OnWeaponDraw()
    {
        IsWeaponDraw = !IsWeaponDraw;
    }
}
