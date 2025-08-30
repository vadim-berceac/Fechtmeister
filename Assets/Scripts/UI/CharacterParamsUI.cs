using UnityEngine;
using UnityEngine.UI;

public class CharacterParamsUI : MonoBehaviour
{
    [field: SerializeField] public GameObject Window { get; set; }
    [field: SerializeField] public Slider Slider { get; set; }
    [field: SerializeField] public Text Text { get; set; }

    private bool _isOpen;
    private CharacterCore _selectedCharacter;

    private void Awake()
    {
        CharacterSelector.OnCharacterSelected += OnCharacterSelected;
        ShowWindow(false);
    }

    private void OnCharacterSelected(CharacterCore character)
    {
        _selectedCharacter = character;
        
        if (_selectedCharacter == null)
        {
            ShowWindow(false);
            return;
        }
        
        ShowWindow(true);
        UpdateText();
        UpdateSlider(_selectedCharacter.Health.CurrentHealth);
    }

    private void ShowWindow(bool show)
    {
        Window.SetActive(show);
        _isOpen = show;

        if (_isOpen)
        {
            SubScribe();
            return;
        }
        UnsubScribe();
    }

    private void SubScribe()
    {
        if (_selectedCharacter == null)
        {
            return;
        }
        _selectedCharacter.Health.OnCurrentHealthChanged += UpdateSlider;
    }

    private void UnsubScribe()
    {
        if (_selectedCharacter == null)
        {
            return;
        }
        _selectedCharacter.Health.OnCurrentHealthChanged -= UpdateSlider;
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

    private void OnDisable()
    {
        CharacterSelector.OnCharacterSelected -= OnCharacterSelected;

        if (_isOpen)
        {
            UnsubScribe();
        }
    }
}
