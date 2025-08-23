using UnityEngine;
using UnityEngine.UIElements;

public class NameUI : MonoBehaviour
{
   [field: SerializeField] private UIDocument UIDocument { get; set; }
   [field: SerializeField] private PickupItem PickupItem { get; set; }

   private void OnEnable()
   {
      var root = UIDocument.rootVisualElement;
      var label = root.Q<Label>("Name");
      label.text = PickupItem.ItemData.ItemName;
   }
}
