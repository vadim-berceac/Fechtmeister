using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InventoryButton : MonoBehaviour
{
   [field: SerializeField] public InventoryButtonSettings Settings { get; set; }

   private IEquppiedItemData _equppiedItemData;
   private InventoryDrawer _inventoryDrawer;

   [Inject]
   private void Construct(InventoryDrawer inventoryDrawer)
   {
      _inventoryDrawer = inventoryDrawer;
   }

   public IEquppiedItemData GetItemData()
   {
      return _equppiedItemData;
   }

   public void SetItemData(IEquppiedItemData equppiedItemData, int count)
   {
      if (equppiedItemData == null)
      {
         _equppiedItemData = null;
         Settings.Image.sprite = null;
         Settings.Text.enabled = false;
         Settings.Count.enabled = false;
         Settings.Button.interactable = false;
         return;
      }
      _equppiedItemData = equppiedItemData;
      Settings.Text.text = _equppiedItemData.ItemName;
      
      if (count > 0)
      {
         Settings.Count.text = count.ToString();
         Settings.Count.enabled = true;
      }
      else
      {
         Settings.Count.enabled = false;
      }
      Settings.Image.sprite = _equppiedItemData.ItemIcon;
      Settings.Text.enabled = true;
     
      Settings.Button.interactable = true;
   }

   public void OnClick()
   {
      if (_equppiedItemData == null)
      {
         return;
      }

      if (_inventoryDrawer.InventoryWeaponButtons.Contains(this))
      {
         Debug.LogWarning("Inventory Button is already in WeaponButtons.");
         _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.InventoryBag.AddItem(_equppiedItemData);
         _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.WeaponSystem.DestroyInstance(_equppiedItemData);
         SetItemData(null, 0);
         _inventoryDrawer.UpdateInventoryBag();
         _inventoryDrawer.UpdateWeaponSystem();
         return;
      }

      if (_inventoryDrawer.InventoryBagButtons.Contains(this))
      {

         if (_equppiedItemData is WeaponData)
         {
            if (!_inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.WeaponSystem.Equip(_equppiedItemData, 
                   _inventoryDrawer.InventoryUI.CurrentCharacter.LocomotionSettings.CharacterCollider))
            {
               return;
            }
            _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.InventoryBag.RemoveItem(_equppiedItemData);
            _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.WeaponSystem.SelectWeapon(0);
            SetItemData(null, 0);
            _inventoryDrawer.UpdateInventoryBag();
            _inventoryDrawer.UpdateWeaponSystem();
            return;
         }

         if (_equppiedItemData is ArmorData)
         {
            if (!_inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.ArmorSystem.Equip(_equppiedItemData,
                   _inventoryDrawer.InventoryUI.CurrentCharacter.LocomotionSettings.CharacterCollider))
            {
               return;
            }
            _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.InventoryBag.RemoveItem(_equppiedItemData);
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
