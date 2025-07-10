using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, ICharacterInputSet
{
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private PlayerActionsNames playerActionsNames;
    
    private readonly List<InputAction> _actions = new ();
    private InputAction _onAttack;
    private InputAction _onInteract;
    private InputAction _onJump;
    private InputAction _onSneak;
    private InputAction _onSprint;
    private InputAction _onDrawWeapon;
    private InputAction _onHoldTarget;
    private InputAction _onOpenInventory;
    private InputAction _onWeaponSelect0;
    private InputAction _onWeaponSelect1;
    private InputAction _onWeaponSelect2;
    private InputAction _onMove;
    private InputAction _onLook;
    public event Action OnAttack;
    public event Action OnInteract;
    public event Action OnJump;
    public event Action OnSneak;
    public event Action OnRun;
    public event Action OnDrawWeapon;
    public event Action OnHoldTarget;
    public event Action OnOpenInventory;
    public event Action OnWeaponSelect0;
    public event Action OnWeaponSelect1;
    public event Action OnWeaponSelect2;
    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    
    private void Awake()
    {
        FindActions();
        Enable();
        Subscribe();
    }

    public void FindActions()
    {
        if (inputActionAsset == null)
        {
            Debug.LogError("InputActionAsset is not assigned in the inspector.", this);
            return;
        }
        
        _onAttack = inputActionAsset.FindAction(playerActionsNames.Attack);
        _onInteract = inputActionAsset.FindAction(playerActionsNames.Interact);
        _onJump = inputActionAsset.FindAction(playerActionsNames.Jump);
        _onSneak = inputActionAsset.FindAction(playerActionsNames.Sneak);
        _onSprint = inputActionAsset.FindAction(playerActionsNames.Sprint);
        _onDrawWeapon = inputActionAsset.FindAction(playerActionsNames.DrawWeapon);
        _onHoldTarget = inputActionAsset.FindAction(playerActionsNames.HoldTarget);
        _onOpenInventory = inputActionAsset.FindAction(playerActionsNames.OpenInventory);
        _onWeaponSelect0 = inputActionAsset.FindAction(playerActionsNames.WeaponSelect0);
        _onWeaponSelect1 = inputActionAsset.FindAction(playerActionsNames.WeaponSelect1);
        _onWeaponSelect2 = inputActionAsset.FindAction(playerActionsNames.WeaponSelect2);
        _onMove =  inputActionAsset.FindAction(playerActionsNames.Move);
        _onLook =  inputActionAsset.FindAction(playerActionsNames.Look);

        _actions.Clear();
        _actions.AddRange(new[]
        {
            _onAttack, _onInteract, _onJump, _onSneak, _onSprint, _onDrawWeapon, _onHoldTarget,
            _onOpenInventory, _onWeaponSelect0, _onWeaponSelect1, _onWeaponSelect2, _onMove, _onLook
        });
    }

    public void Subscribe()
    {
        _onAttack.performed += OnAttackCTX;
        _onInteract.performed += OnInteractCTX;
        _onSneak.performed += OnSneakCTX;
        _onDrawWeapon.performed += OnDrawWeaponCTX;
        _onHoldTarget.performed += OnHoldTargetCTX;
        _onOpenInventory.performed += OnOpenInventoryCTX;
        _onWeaponSelect0.performed += OnWeaponSelect0CTX;
        _onWeaponSelect1.performed += OnWeaponSelect1CTX;
        _onWeaponSelect2.performed += OnWeaponSelect2CTX;
        
        _onSprint.performed += OnSprintCTX;
        _onSprint.canceled += OnSprintCTXCancel;
        
        _onMove.performed += OnMoveCTX;
        _onMove.canceled += OnMoveCTXCancel;
        
        _onJump.performed += OnJumpCTX;
        _onJump.canceled += OnJumpCTXCancel;
        
        _onLook.performed += OnLookCTX;
        _onLook.canceled += OnLookCTXCancel;
    }

    public void Unsubscribe()
    {
        _onAttack.performed -= OnAttackCTX;
        _onInteract.performed -= OnInteractCTX;
        _onSneak.performed -= OnSneakCTX;
        _onDrawWeapon.performed -= OnDrawWeaponCTX;
        _onHoldTarget.performed -= OnHoldTargetCTX;
        _onOpenInventory.performed -= OnOpenInventoryCTX;
        _onWeaponSelect0.performed -= OnWeaponSelect0CTX;
        _onWeaponSelect1.performed -= OnWeaponSelect1CTX;
        _onWeaponSelect2.performed -= OnWeaponSelect2CTX;
        
        _onSprint.performed -= OnSprintCTX;
        _onSprint.canceled -= OnSprintCTXCancel;
        
        _onMove.performed -= OnMoveCTX;
        _onMove.canceled -= OnMoveCTXCancel;
        
        _onJump.performed -= OnJumpCTX;
        _onJump.canceled -= OnJumpCTXCancel;
        
        _onLook.performed -= OnLookCTX;
        _onLook.canceled -= OnLookCTXCancel;
    }

    public void Enable()
    {
        foreach (var action in _actions)
        {
            action?.Enable();
        }
    }

    public void Disable()
    {
        foreach (var action in _actions)
        {
            action?.Disable();
        }
    }
    
    private void OnAttackCTX(InputAction.CallbackContext ctx)
    {
        OnAttack?.Invoke();
    }

    private void OnInteractCTX(InputAction.CallbackContext ctx)
    {
        OnInteract?.Invoke();
    }

    private void OnJumpCTX(InputAction.CallbackContext ctx)
    {
        OnJump?.Invoke();
    }
    
    private void OnJumpCTXCancel(InputAction.CallbackContext ctx)
    {
        OnJump?.Invoke();
    }

    private void OnSneakCTX(InputAction.CallbackContext ctx)
    {
        OnSneak?.Invoke();
    }

    private void OnSprintCTX(InputAction.CallbackContext ctx)
    {
        OnRun?.Invoke();
    }

    private void OnSprintCTXCancel(InputAction.CallbackContext ctx)
    {
        OnRun?.Invoke();
    }

    private void OnDrawWeaponCTX(InputAction.CallbackContext ctx)
    {
        OnDrawWeapon?.Invoke();
    }

    private void OnHoldTargetCTX(InputAction.CallbackContext ctx)
    {
        OnHoldTarget?.Invoke();
    }

    private void OnOpenInventoryCTX(InputAction.CallbackContext ctx)
    {
        OnOpenInventory?.Invoke();
    }

    private void OnWeaponSelect0CTX(InputAction.CallbackContext ctx)
    {
        OnWeaponSelect0?.Invoke();
    }

    private void OnWeaponSelect1CTX(InputAction.CallbackContext ctx)
    {
        OnWeaponSelect1?.Invoke();
    }
    
    private void OnWeaponSelect2CTX(InputAction.CallbackContext ctx)
    {
        OnWeaponSelect2?.Invoke();
    }

    private void OnMoveCTX(InputAction.CallbackContext ctx)
    {
        OnMove?.Invoke(ctx.ReadValue<Vector2>());
    }

    private void OnMoveCTXCancel(InputAction.CallbackContext ctx)
    {
        OnMove?.Invoke(Vector2.zero);
    }

    private void OnLookCTX(InputAction.CallbackContext ctx)
    {
        OnLook?.Invoke(ctx.ReadValue<Vector2>());
    }

    private void OnLookCTXCancel(InputAction.CallbackContext ctx)
    {
        OnLook?.Invoke(Vector2.zero);
    }
    
    private void OnDisable()
    {
        Unsubscribe();
        Disable();
    }

    private void OnDestroy()
    {
        OnAttack = null;
        OnInteract = null;
        OnJump = null;
        OnSneak = null;
        OnRun = null;
        OnHoldTarget = null;
        OnOpenInventory = null;
        OnWeaponSelect0 = null;
        OnWeaponSelect1 = null;
        OnWeaponSelect2 = null;
        OnMove = null;
        OnLook = null;
    }
}

[System.Serializable]
public struct PlayerActionsNames
{
    [field: SerializeField] public string Move { get; private set; }
    [field: SerializeField] public string Look { get; private set; }
    [field: SerializeField] public string Attack { get; private set; }
    [field: SerializeField] public string Interact { get; private set; }
    [field: SerializeField] public string Jump { get; private set; }
    [field: SerializeField] public string Sprint { get; private set; }
    [field: SerializeField] public string Sneak { get; private set; }
    [field: SerializeField] public string HoldTarget { get; private set; }
    [field: SerializeField] public string DrawWeapon { get; private set; }
    [field: SerializeField] public string OpenInventory { get; private set; }
    [field: SerializeField] public string WeaponSelect0 { get; private set; }
    [field: SerializeField] public string WeaponSelect1 { get; private set; }
    [field: SerializeField] public string WeaponSelect2 { get; private set; }
}
