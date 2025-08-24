using System.Text.RegularExpressions;
using Unity.Burst;
using UnityEngine;
using UnityEngine.UIElements;

public class NameUI : MonoBehaviour
{
   [field: SerializeField] private UIDocument UIDocument { get; set; }
   private IItemData _itemData;
   private const string LabelName = "Name";
   private const string Pattern = @"^(.+)$";
   private const string Replacement = "[$1]";

   private void OnEnable()
   {
      SetValues();
   }
   
   public void Set(IItemData item)
   {
      _itemData = item;
   }
   
   [BurstCompile]
   private void SetValues()
   {
      if (_itemData == null)
      {
         return;
      }
      var root = UIDocument.rootVisualElement;
      var label = root.Q<Label>(LabelName);
      var formattedText = Regex.Replace(_itemData.ItemName, Pattern, Replacement);
      label.text = formattedText;
   }
}
