using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class InventoryButton : MonoBehaviour
{
   [field: SerializeField] public Button Button { get; set; }
   [field: SerializeField] public Text Text { get; set; }
   [field: SerializeField] public Image Image { get; set; }
   
   public IItemData ItemData { get; set; }
   private InventoryDrawer _inventoryDrawer;

   [Inject]
   private void Construct(InventoryDrawer inventoryDrawer)
   {
      _inventoryDrawer = inventoryDrawer;
   }

   public void SetItemData(IItemData itemData)
   {
      if (itemData == null)
      {
         ItemData = null;
         Image.sprite = null;
         Text.enabled = false;
         Button.interactable = false;
         return;
      }
      ItemData = itemData;
      Text.text = ItemData.ItemName;
      Image.sprite = ItemData.ItemIcon;
      Text.enabled = true;
      Button.interactable = true;
   }

   public void OnClick()
   {
      if (ItemData == null)
      {
         return;
      }

      if (_inventoryDrawer.InventoryWeaponButtons.Contains(this))
      {
         Debug.LogWarning("Inventory Button is already in WeaponButtons.");
         _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.InventoryBag.AddItem(ItemData);
         _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.WeaponSystem.DestroyInstance(ItemData);
         SetItemData(null);
         _inventoryDrawer.UpdateInventoryBag();
         _inventoryDrawer.UpdateWeaponSystem();
         return;
      }

      if (_inventoryDrawer.InventoryBagButtons.Contains(this))
      {
         Debug.LogWarning("Inventory Button is already in BagButtons.");

         if (ItemData is WeaponData)
         {
            _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.InventoryBag.RemoveItem(ItemData);
            _inventoryDrawer.InventoryUI.CurrentCharacter.Inventory.WeaponSystem.Equip(ItemData);
            SetItemData(null);
            _inventoryDrawer.UpdateInventoryBag();
            _inventoryDrawer.UpdateWeaponSystem();
            return;
         }

         // if (ItemData is ArmorData)
         // {
         //    return;
         // }
         
         //if ItemData is Arrow
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
