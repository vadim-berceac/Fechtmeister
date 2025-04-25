using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, ICharacterInputSet
{
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private PlayerActionsNames playerActionsNames;
    
    public event Action OnAttack;
    public event Action OnInteract;
    public event Action OnJump;
    public event Action OnSneak;
    public event Action OnSprint;
    public event Action OmDrawWeapon;
    public event Action OnHoldTarget;
    public event Action OnOpenInventory;
    public event Action OnWeaponSelect0;
    public event Action OnWeaponSelect1;
    public event Action OnWeaponSelect2;
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    
    private void Awake()
    {
        FindActions();
        Enable();
        Subscribe();
        Debug.LogWarning("PlayerInput: Awake");
    }

    public void FindActions()
    {
        
    }

    public void Subscribe()
    {
        
    }

    public void Unsubscribe()
    {
        
    }

    public void Enable()
    {
        
    }

    public void Disable()
    {
        
    }

    private void OnDisable()
    {
        Unsubscribe();
        Disable();
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
