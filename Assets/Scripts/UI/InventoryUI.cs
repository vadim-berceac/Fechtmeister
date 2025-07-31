using UnityEngine;
using Zenject;

public class InventoryUI : MonoBehaviour, IGameWindow
{
    private PlayerInput _playerInput;
    private GameWindowContainer _gameWindowContainer;
    private SceneCamera _sceneCamera;
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
    }

    private void OnInventoryInvoked()
    {
        if (_sceneCamera.Target == null)
        {
            Close();
            return;
        }
        
        var currentCharacter = _sceneCamera.Target.GetComponent<CharacterCore>();
        
        if (IsActive())
        {
            Close();
            currentCharacter.CharacterInputHandler.InventoryOpen(false);
            return;
        }
       
        currentCharacter.CharacterInputHandler.InventoryOpen(true);
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
        inventoryWindow.SetActive(true);
        BLockPlayerInput(true);
    }

    public void Close()
    {
        if (!IsActive())
        {
            return;
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
    }
}
