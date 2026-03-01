using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InventoryButton : MonoBehaviour
{
    [field: SerializeField] public InventoryButtonSettings Settings { get; set; }

    private ISimpleItemData _equppiedItemData;
    private InventoryDrawer _inventoryDrawer;
    private Inventory CharacterInventory => _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory;

    [Inject]
    private void Construct(InventoryDrawer inventoryDrawer)
    {
        _inventoryDrawer = inventoryDrawer;
    }

    public ISimpleItemData GetItemData() => _equppiedItemData;

    public void SetItemData(ISimpleItemData equppiedItemData, int count)
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

    private void UpdateButtonVisuals(ISimpleItemData itemData, int count)
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
        else if (_inventoryDrawer.InventoryProjectilesButtons.Contains(this))
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
        CharacterInventory.InventoryBag.AddItem(_equppiedItemData, 1);
        CharacterInventory.WeaponSystem.DestroyInstance(_equppiedItemData);
        
        SetItemData(null, 0);
        _inventoryDrawer.UpdateInventoryBag();
        _inventoryDrawer.UpdateWeaponSystem();
    }

    private void HandleArmorUnEquip()
    {
        CharacterInventory.InventoryBag.AddItem(_equppiedItemData, 1);
        CharacterInventory.ArmorSystem.DestroyInstance(_equppiedItemData);
        
        SetItemData(null, 0);
        _inventoryDrawer.UpdateInventoryBag();
        _inventoryDrawer.UpdateArmorSystem();
    }

    private void HandleProjectilesUnEquip()
    {
        var quantity = CharacterInventory.ProjectileSystem.GetCell(_equppiedItemData).Quantity;
        CharacterInventory.InventoryBag.AddItem(_equppiedItemData, quantity);
        CharacterInventory.ProjectileSystem.RemoveItem(_equppiedItemData, quantity);
        
        CharacterInventory.ProjectileSystem.DestroyInstance(_equppiedItemData);
        
        SetItemData(null, 0);
        _inventoryDrawer.UpdateInventoryBag();
        _inventoryDrawer.UpdateProjectiles();
    }

    private void EquipWeapon()
    {
        var character = _inventoryDrawer.InventoryUI.CurrentCharacter;
        
        if (!CharacterInventory.WeaponSystem.Equip(
            _equppiedItemData as IEquppiedItemData, 
            character.CapsuleCollider,
            character.GraphCore.Animator))
        {
            return;
        }

        CharacterInventory.InventoryBag.RemoveItem(_equppiedItemData, 1);
        CharacterInventory.WeaponSystem.SelectWeapon(0);
        
        SetItemData(null, 0);
        _inventoryDrawer.UpdateInventoryBag();
        _inventoryDrawer.UpdateWeaponSystem();
    }

    private void EquipArmor()
    {
        var character = _inventoryDrawer.InventoryUI.CurrentCharacter;
        
        if (!CharacterInventory.ArmorSystem.Equip(
            _equppiedItemData as IEquppiedItemData,
            character.CapsuleCollider,
            character.GraphCore.Animator))
        {
            return;
        }

        CharacterInventory.InventoryBag.RemoveItem(_equppiedItemData, 1);
        
        SetItemData(null, 0);
        _inventoryDrawer.UpdateInventoryBag();
        _inventoryDrawer.UpdateArmorSystem();
    }

    private void EquipProjectiles()
    {
        var character = _inventoryDrawer.InventoryUI.CurrentCharacter;
        
        if (!CharacterInventory.ProjectileSystem.Equip(
                _equppiedItemData as IEquppiedItemData,
                character.CapsuleCollider,
                character.GraphCore.Animator))
        {
            return;
        }

        var quantity = CharacterInventory.InventoryBag.GetCell(_equppiedItemData).Quantity;
        CharacterInventory.InventoryBag.RemoveItem(_equppiedItemData, quantity);
        CharacterInventory.ProjectileSystem.AddItem(_equppiedItemData, quantity);
        
        SetItemData(null, 0);
        _inventoryDrawer.UpdateInventoryBag();
        _inventoryDrawer.UpdateProjectiles();
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