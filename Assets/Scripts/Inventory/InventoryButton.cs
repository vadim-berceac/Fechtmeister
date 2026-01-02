using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InventoryButton : MonoBehaviour
{
    [field: SerializeField] public InventoryButtonSettings Settings { get; set; }

    private IEquppiedItemData _equppiedItemData;
    private InventoryDrawer _inventoryDrawer;
    private Inventory CharacterInventory => _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory;

    [Inject]
    private void Construct(InventoryDrawer inventoryDrawer)
    {
        _inventoryDrawer = inventoryDrawer;
    }

    public IEquppiedItemData GetItemData() => _equppiedItemData;

    public void SetItemData(IEquppiedItemData equppiedItemData, int count)
    {
        _equppiedItemData = equppiedItemData;

        if (equppiedItemData == null)
        {
            ClearButton();
            return;
        }

        UpdateButtonVisuals(equppiedItemData, count);
    }

    private void ClearButton()
    {
        Settings.Image.sprite = null;
        Settings.Text.enabled = false;
        Settings.Count.enabled = false;
        Settings.Button.interactable = false;
    }

    private void UpdateButtonVisuals(IEquppiedItemData itemData, int count)
    {
        Settings.Image.sprite = itemData.ItemIcon;
        Settings.Text.text = itemData.ItemName;
        Settings.Text.enabled = true;
        Settings.Button.interactable = true;

        Settings.Count.enabled = count > 0;
        if (count > 0)
        {
            Settings.Count.text = count.ToString();
        }
    }

    public void OnClick()
    {
        if (_equppiedItemData == null) return;

        if (_inventoryDrawer.InventoryBagButtons.Contains(this))
        {
            HandleBagItemEquip();
        }
        else if (_inventoryDrawer.InventoryWeaponButtons.Contains(this))
        {
            HandleWeaponUnEquip();
        }
        else if (_inventoryDrawer.InventoryArmorButtons.Contains(this))
        {
            HandleArmorUnEquip();
        }
        else if (_inventoryDrawer.InventoryArrowButtons.Contains(this))
        {
            HandleProjectilesUnEquip();
        }
    }
    
    private void HandleBagItemEquip()
    {
        switch (_equppiedItemData)
        {
            case WeaponData:
                EquipWeapon();
                break;
            case ArmorData:
                EquipArmor();
                break;
            case ProjectileData:
                EquipProjectiles();
                break;
        }
    }

    private void HandleWeaponUnEquip()
    {
        //Debug.LogWarning("Inventory Button is already in WeaponButtons.");
        
        CharacterInventory.InventoryBag.AddItem(_equppiedItemData);
        CharacterInventory.WeaponSystem.DestroyInstance(_equppiedItemData);
        
        SetItemData(null, 0);
        _inventoryDrawer.UpdateInventoryBag();
        _inventoryDrawer.UpdateWeaponSystem();
    }

    private void HandleArmorUnEquip()
    {
        //Debug.LogWarning("Inventory Button is already in ArmorButtons.");
      
        CharacterInventory.InventoryBag.AddItem(_equppiedItemData);
        CharacterInventory.ArmorSystem.DestroyInstance(_equppiedItemData);
        
        SetItemData(null, 0);
        _inventoryDrawer.UpdateInventoryBag();
        _inventoryDrawer.UpdateArmorSystem();
    }

    private void HandleProjectilesUnEquip()
    {
        Debug.LogWarning("Inventory Button is already in ArrowButtons.");
        // TODO: Implement arrow action logic
    }

    private void EquipWeapon()
    {
        var character = _inventoryDrawer.InventoryUI.CurrentCharacter;
        
        if (!CharacterInventory.WeaponSystem.Equip(
            _equppiedItemData,
            character.LocomotionSettings.CharacterCollider,
            character.GraphCore.CoreData.Animator))
        {
            return;
        }

        CharacterInventory.InventoryBag.RemoveItem(_equppiedItemData);
        CharacterInventory.WeaponSystem.SelectWeapon(0);
        
        SetItemData(null, 0);
        _inventoryDrawer.UpdateInventoryBag();
        _inventoryDrawer.UpdateWeaponSystem();
    }

    private void EquipArmor()
    {
        var character = _inventoryDrawer.InventoryUI.CurrentCharacter;
        
        if (!CharacterInventory.ArmorSystem.Equip(
            _equppiedItemData,
            character.LocomotionSettings.CharacterCollider,
            character.GraphCore.CoreData.Animator))
        {
            return;
        }

        CharacterInventory.InventoryBag.RemoveItem(_equppiedItemData);
        
        SetItemData(null, 0);
        _inventoryDrawer.UpdateInventoryBag();
        _inventoryDrawer.UpdateArmorSystem();
    }

    private void EquipProjectiles()
    {
        // TODO: Implement arrow action logic
    }
}

[System.Serializable]
public struct InventoryButtonSettings
{
    [field: SerializeField] public Button Button { get; set; }
    [field: SerializeField] public Text Text { get; set; }
    [field: SerializeField] public Text Count { get; set; }
    [field: SerializeField] public Image Image { get; set; }
}