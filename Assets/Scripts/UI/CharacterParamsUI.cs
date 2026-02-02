using UnityEngine;
using UnityEngine.UI;

public class CharacterParamsUI : MonoBehaviour
{
    [field: SerializeField] public GameObject Window { get; set; }
    [field: SerializeField] public Slider Slider { get; set; }
    [field: SerializeField] public Text Text { get; set; }
    
    [Header("Debug")]
    [field: SerializeField] public bool Debug { get; set; }
    [field: SerializeField] public GameObject DebugWindow { get; set; }
    [field: SerializeField] public Text DebugText { get; set; }

    private bool _isOpen;
    private CharacterCore _selectedCharacter;

    private void Start()
    {
        CharacterSelector.OnCharacterSelected += OnCharacterSelected;
        ShowWindow(false);
        DebugWindow.SetActive(Debug);
    }

    private void OnCharacterSelected(CharacterInfo character)
    {
        if (character == null)
        {
            return;
        }
        UnsubScribe();
        _selectedCharacter = character.Core;
        
        if (_selectedCharacter == null)
        {
            ShowWindow(false);
            return;
        }
        SubScribe();
        ShowWindow(true);
        UpdateText();
        UpdateSlider(_selectedCharacter.Health.CurrentHealth);
        UpdateState();
    }

    private void ShowWindow(bool show)
    {
        Window.SetActive(show);
        _isOpen = show;
    }

    private void SubScribe()
    {
        if (_selectedCharacter == null)
        {
            return;
        }
        _selectedCharacter.Health.OnCurrentHealthChanged += UpdateSlider;
        _selectedCharacter.OnStateChanged += UpdateState;
    }

    private void UnsubScribe()
    {
        if (_selectedCharacter == null)
        {
            return;
        }
        _selectedCharacter.Health.OnCurrentHealthChanged -= UpdateSlider;
        _selectedCharacter.OnStateChanged -= UpdateState;
    }

    private void UpdateSlider(float currentValue)
    {
        Slider.value = currentValue / _selectedCharacter.Health.MaxHealth;
    }

    private void UpdateText()
    {
        if (_selectedCharacter == null)
        {
            Text.text = "";
            return;
        }
        Text.text = _selectedCharacter.gameObject.name;
    }

    private void UpdateState()
    {
        if (_selectedCharacter == null || _selectedCharacter.CurrentState == null)
        {
            DebugText.text = "";
            return;
        }
        DebugText.text = _selectedCharacter.CurrentState.name;
    }

    private void OnDisable()
    {
        CharacterSelector.OnCharacterSelected -= OnCharacterSelected;

        if (_isOpen)
        {
            UnsubScribe();
        }
    }
}
