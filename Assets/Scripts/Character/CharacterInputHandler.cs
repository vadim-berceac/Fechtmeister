using Unity.Burst;
using UnityEngine;

public class CharacterInputHandler : IInputHandler
{
    public IInputSet InputSet { get; private set; }
    private ICharacterInputSet _characterInputSet;

    public bool IsAttack { get; private set; }
    public bool IsRun { get; private set; }
    public bool IsJump { get; private set; }
    public bool IsWeaponDraw { get; private set; }
    public float InputX { get; private set; }
    public float InputY { get; private set; }
    public float LookX { get; private set; }
   
    private float _targetInputX;
    private float _targetInputY;
    
    private readonly float _smoothingSpeed; 
    
    private bool _isSubscribed;

    public CharacterInputHandler(float smoothingSpeed)
    {
        _smoothingSpeed = smoothingSpeed;
    }

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
    
    [BurstCompile]
    public void SmoothInput(float deltaTime)
    {
        if (InputSet == null || _characterInputSet == null)
        {
            return;
        }
        InputX = Mathf.Lerp(InputX, _targetInputX, _smoothingSpeed * deltaTime);
        InputY = Mathf.Lerp(InputY, _targetInputY, _smoothingSpeed * deltaTime);
       
        if (Mathf.Abs(InputX - _targetInputX) < 0.01f) InputX = _targetInputX;
        if (Mathf.Abs(InputY - _targetInputY) < 0.01f) InputY = _targetInputY;
    }

    private void Subscribe()
    {
        if (_isSubscribed)
        {
            return;
        }
        _isSubscribed = true;

        _characterInputSet.OnMove += OnMove;
        _characterInputSet.OnJump += OnJump;
        _characterInputSet.OnRun += OnRun;
        _characterInputSet.OnLook += OnLook;
        _characterInputSet.OnDrawWeapon += OnWeaponDraw;
        _characterInputSet.OnAttack += OnAttack;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed)
        {
            return;
        }
        _isSubscribed = false;

        _characterInputSet.OnMove -= OnMove;
        _characterInputSet.OnJump -= OnJump;
        _characterInputSet.OnRun -= OnRun;
        _characterInputSet.OnLook -= OnLook;
        _characterInputSet.OnDrawWeapon -= OnWeaponDraw;
        _characterInputSet.OnAttack -= OnAttack;
    }

    private void OnMove(Vector2 move)
    {
        _targetInputX = move.x;
        _targetInputY = move.y;
    }

    private void OnJump()
    {
        IsJump = !IsJump;
    }

    private void OnLook(Vector2 look)
    {
        LookX = look.x;
    }

    private void OnRun()
    {
        IsRun = !IsRun;
    }

    private void OnWeaponDraw()
    {
        IsWeaponDraw = !IsWeaponDraw;
    }

    private void OnAttack()
    {
        IsAttack = true;
    }

    public void ResetAttack()
    {
        IsAttack = false;
    }
}
