using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InventoryButton : MonoBehaviour
{
   [field: SerializeField] public Button Button { get; set; }
   [field: SerializeField] public Text Text { get; set; }
   [field: SerializeField] public Image Image { get; set; }

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

   public void SetItemData(IItemData itemData)
   {
      if (itemData == null)
      {
         _itemData = null;
         Image.sprite = null;
         Text.enabled = false;
         Button.interactable = false;
         return;
      }
      _itemData = itemData;
      Text.text = _itemData.ItemName;
      Image.sprite = _itemData.ItemIcon;
      Text.enabled = true;
      Button.interactable = true;
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
         SetItemData(null);
         _inventoryDrawer.UpdateInventoryBag();
         _inventoryDrawer.UpdateWeaponSystem();
         return;
      }

      if (_inventoryDrawer.InventoryBagButtons.Contains(this))
      {
         Debug.LogWarning("Inventory Button is already in BagButtons.");

         if (_itemData is WeaponData)
         {
            if (!_inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.WeaponSystem.Equip(_itemData))
            {
               return;
            }
            _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.InventoryBag.RemoveItem(_itemData);
            _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.WeaponSystem.SelectWeapon(0);
            SetItemData(null);
            _inventoryDrawer.UpdateInventoryBag();
            _inventoryDrawer.UpdateWeaponSystem();
            return;
         }

         // if (_itemData is ArmorData)
         // {
         //    return;
         // }
         
         //if _itemData is Arrow
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
