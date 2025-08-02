using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDrawer : MonoBehaviour
{
    [field: SerializeField] public InventoryUI InventoryUI { get; set; }
   
    private void OnEnable()
    {
        UpdateInventoryCells();
    }

    private void UpdateInventoryCells()
    {
       UpdateInventoryBag();
       UpdateWeaponSystem();
       UpdateArmorSystem();
       UpdateArrows();
       UpdateMoney();
    }

    public void UpdateInventoryBag()
    {
        Debug.Log($"Показываю InventoryBag {InventoryUI.CurrentCharacter.name}");
    }

    public void UpdateWeaponSystem()
    {
        var w = InventoryUI.CurrentCharacter.Inventory.WeaponSystem.InstanceInHands.ItemData.ItemName;
        Debug.Log($"{w} WeaponSystem {InventoryUI.CurrentCharacter.name}");
    }

    public void UpdateArmorSystem()
    {
        Debug.Log($"Показываю ArmorSystem {InventoryUI.CurrentCharacter.name}");
    }

    public void UpdateArrows()
    {
        Debug.Log($"Показываю Arrows {InventoryUI.CurrentCharacter.name}");
    }

    public void UpdateMoney()
    {
        Debug.Log($"Показываю Money {InventoryUI.CurrentCharacter.name}");
    }
}
