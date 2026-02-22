using System.Text.RegularExpressions;
using Unity.Burst;
using UnityEngine;
using UnityEngine.UIElements;

public class NameUI : MonoBehaviour
{
   [field: SerializeField] private UIDocument UIDocument { get; set; }
   private ISimpleItemData _equppiedItemData;
   private const string LabelName = "Name";
   private const string Pattern = @"^(.+)$";
   private const string Replacement = "[$1]";

   private void OnEnable()
   {
      SetValues();
   }
   
   public void Set(ISimpleItemData equppiedItem)
   {
      _equppiedItemData = equppiedItem;
   }
   
   private void SetValues()
   {
      if (_equppiedItemData == null)
      {
         return;
      }
      var root = UIDocument.rootVisualElement;
      var label = root.Q<Label>(LabelName);
      var formattedText = Regex.Replace(_equppiedItemData.ItemName, Pattern, Replacement);
      label.text = formattedText;
   }
}
