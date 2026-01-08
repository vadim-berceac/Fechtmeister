using System;
using Unity.Burst;
using UnityEngine;

public class CharacterInputHandler : IInputHandler
{
    public IInputSet InputSet { get; private set; }
    private ICharacterInputSet _characterInputSet;

    public bool IsAttack { get; private set; }
    public bool IsAimBlock { get; private set; }
    public bool IsInteract { get; private set; }
    public bool IsRun { get; private set; }
    public bool IsJump { get; private set; }
    public bool IsInventoryOpen { get; private set; }
    public bool IsWeaponDraw { get; private set; }
    public float InputX { get; private set; }
    public float InputY { get; private set; }
    public float LookX { get; private set; }
    
    public Vector3 DirVector3  { get; private set; }
    public float TargetInputMagnitude { get; private set; }
   
    private float _targetInputX;
    private float _targetInputY;
    
    private readonly float _smoothingSpeed; 
    
    private bool _isSubscribed;
    public event Action<int> OnWeaponSwitch;

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
        
        DirVector3 = new Vector3(InputX, 0, InputY);
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
        _characterInputSet.OnInteract += OnInteract;
        _characterInputSet.OnWeaponSelect += OnWeaponSelect;
        _characterInputSet.OnAimBlock += OnAimBlock;
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
        _characterInputSet.OnInteract -= OnInteract;
        _characterInputSet.OnWeaponSelect -= OnWeaponSelect;
        _characterInputSet.OnAimBlock -= OnAimBlock;
    }

    private void OnMove(Vector2 move)
    {
        _targetInputX = move.x;
        _targetInputY = move.y;
        
        TargetInputMagnitude = move.magnitude;
    }

    private void OnJump()
    {
        IsJump = true;
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

    private void OnAimBlock()
    {
        IsAimBlock = !IsAimBlock;
    }

    public void ResetInputBuffer()
    {
        IsAttack = false;
        IsInteract = false;
        IsJump = false;
    }

    private void OnInteract()
    {
        IsInteract = true;
    }

    public void InventoryOpen(bool value)
    {
        IsInventoryOpen = value;
    }

    private void OnWeaponSelect(int index)
    {
        OnWeaponSwitch?.Invoke(index);
    }
}
