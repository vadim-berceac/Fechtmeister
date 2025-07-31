using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class InventoryUI : MonoBehaviour, IGameWindow
{
    private PlayerInput _playerInput;
    private GameWindowContainer _gameWindowContainer;
    private SceneCamera _sceneCamera;
    public CharacterCore CurrentCharacter { get; private set; }
    
    [SerializeField] private GameObject inventoryWindow;

    [Inject]
    private void Construct(PlayerInput playerInput, GameWindowContainer gameWindowContainer, SceneCamera sceneCamera)
    {
        _playerInput = playerInput;
        _gameWindowContainer = gameWindowContainer;
        _sceneCamera = sceneCamera;
    }

    private void Awake()
    {
        Close();
        Register();

        _playerInput.OnOpenInventory += OnInventoryInvoked;
        _sceneCamera.OnTargetChanged += Close;
    }

    private void OnInventoryInvoked()
    {
        if (_sceneCamera.Target == null)
        {
            Close();
            return;
        }
        
        CurrentCharacter = _sceneCamera.Target.GetComponent<CharacterCore>();
        
        if (IsActive())
        {
            Close();
            return;
        }
        Open();
    }

    public bool IsActive()
    {
        return inventoryWindow.activeSelf;
    }

    public void Open()
    {
        if (IsActive())
        {
            return;
        }
        
        foreach (var window in _gameWindowContainer.GameWindows)
        {
            window.Close(); 
        }

        if (CurrentCharacter != null)
        {
            CurrentCharacter.CharacterInputHandler.InventoryOpen(true);
        }
        
        inventoryWindow.SetActive(true);
        BLockPlayerInput(true);
    }

    public void Close()
    {
        if (!IsActive())
        {
            return;
        }

        if (CurrentCharacter != null)
        {
            CurrentCharacter.CharacterInputHandler.InventoryOpen(false);
        }
        
        inventoryWindow.SetActive(false);
        BLockPlayerInput(false);
    }

    public void Register()
    {
        _gameWindowContainer.GameWindows.Add(this);
    }

    public void BLockPlayerInput(bool value)
    {
        if (value)
        {
            _playerInput.Disable();
            _playerInput.EnableByIndex(7);
            return;
        }
        _playerInput.DisableByIndex(7);
        _playerInput.Enable();
    }

    private void OnDisable()
    {
        _playerInput.OnOpenInventory -= OnInventoryInvoked;
        _sceneCamera.OnTargetChanged -= Close;
    }
}
