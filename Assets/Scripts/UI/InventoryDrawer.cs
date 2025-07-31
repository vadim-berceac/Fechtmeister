using UnityEngine;

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

    private void UpdateInventoryBag()
    {
        Debug.Log($"Показываю InventoryBag {InventoryUI.CurrentCharacter.name}");
    }

    private void UpdateWeaponSystem()
    {
        Debug.Log($"Показываю WeaponSystem {InventoryUI.CurrentCharacter.name}");
    }

    private void UpdateArmorSystem()
    {
        Debug.Log($"Показываю ArmorSystem {InventoryUI.CurrentCharacter.name}");
    }

    private void UpdateArrows()
    {
        Debug.Log($"Показываю Arrows {InventoryUI.CurrentCharacter.name}");
    }

    private void UpdateMoney()
    {
        Debug.Log($"Показываю Money {InventoryUI.CurrentCharacter.name}");
    }
}
