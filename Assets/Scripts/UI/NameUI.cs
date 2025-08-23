using UnityEngine;
using UnityEngine.UIElements;

public class NameUI : MonoBehaviour
{
   [field: SerializeField] private UIDocument UIDocument { get; set; }
  
   private IItemData _itemData;

   private void OnEnable()
   {
      SetValues();
   }
   
   public void Set(IItemData item)
   {
      _itemData = item;
   }
   private void SetValues()
   {
      if (_itemData == null)
      {
         return;
      }
      var root = UIDocument.rootVisualElement;
      var label = root.Q<Label>("Name");
      label.text = _itemData.ItemName;
   }
}
