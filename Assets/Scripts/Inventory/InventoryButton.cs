using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
   [field: SerializeField] public Button Button { get; set; }
   [field: SerializeField] public Text Text { get; set; }
   [field: SerializeField] public Image Image { get; set; }
   
   public IItemData ItemData { get; set; }

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
}
