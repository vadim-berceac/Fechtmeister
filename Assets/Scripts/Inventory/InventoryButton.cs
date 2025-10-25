using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InventoryButton : MonoBehaviour
{
   [field: SerializeField] public InventoryButtonSettings Settings { get; set; }

   private IItemData _itemData;
   private InventoryDrawer _inventoryDrawer;

   [Inject]
   private void Construct(InventoryDrawer inventoryDrawer)
   {
      _inventoryDrawer = inventoryDrawer;
   }

   public IItemData GetItemData()
   {
      return _itemData;
   }

   public void SetItemData(IItemData itemData, int count)
   {
      if (itemData == null)
      {
         _itemData = null;
         Settings.Image.sprite = null;
         Settings.Text.enabled = false;
         Settings.Count.enabled = false;
         Settings.Button.interactable = false;
         return;
      }
      _itemData = itemData;
      Settings.Text.text = _itemData.ItemName;
      
      if (count > 0)
      {
         Settings.Count.text = count.ToString();
         Settings.Count.enabled = true;
      }
      else
      {
         Settings.Count.enabled = false;
      }
      Settings.Image.sprite = _itemData.ItemIcon;
      Settings.Text.enabled = true;
     
      Settings.Button.interactable = true;
   }

   public void OnClick()
   {
      if (_itemData == null)
      {
         return;
      }

      if (_inventoryDrawer.InventoryWeaponButtons.Contains(this))
      {
         Debug.LogWarning("Inventory Button is already in WeaponButtons.");
         _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.InventoryBag.AddItem(_itemData);
         _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.WeaponSystem.DestroyInstance(_itemData);
         SetItemData(null, 0);
         _inventoryDrawer.UpdateInventoryBag();
         _inventoryDrawer.UpdateWeaponSystem();
         return;
      }

      if (_inventoryDrawer.InventoryBagButtons.Contains(this))
      {

         if (_itemData is WeaponData)
         {
            if (!_inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.WeaponSystem.Equip(_itemData, 
                   _inventoryDrawer.InventoryUI.CurrentCharacter.LocomotionSettings.CharacterCollider))
            {
               return;
            }
            _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.InventoryBag.RemoveItem(_itemData);
            _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.WeaponSystem.SelectWeapon(0);
            SetItemData(null, 0);
            _inventoryDrawer.UpdateInventoryBag();
            _inventoryDrawer.UpdateWeaponSystem();
            return;
         }

         if (_itemData is ArmorData)
         {
            if (!_inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.ArmorSystem.Equip(_itemData,
                   _inventoryDrawer.InventoryUI.CurrentCharacter.LocomotionSettings.CharacterCollider))
            {
               return;
            }
            _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.InventoryBag.RemoveItem(_itemData);
            SetItemData(null, 0);
            _inventoryDrawer.UpdateInventoryBag();
            _inventoryDrawer.UpdateArmorSystem();
         }
         return;
      }

      if (_inventoryDrawer.InventoryArmorButtons.Contains(this))
      {
         Debug.LogWarning("Inventory Button is already in ArmorButtons.");
         return;
      }
      
      if (_inventoryDrawer.InventoryArrowButtons.Contains(this))
      {
         Debug.LogWarning("Inventory Button is already in ArrowButtons.");
      }
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
