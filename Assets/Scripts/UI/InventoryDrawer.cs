using Unity.Burst;
using UnityEngine;

public class InventoryDrawer : MonoBehaviour
{
    [field: SerializeField] public InventoryUI InventoryUI { get; set; }
    
    [field: SerializeField] public InventoryButton[] InventoryBagButtons { get; set; }
    [field: SerializeField] public InventoryButton[] InventoryWeaponButtons { get; set; }
    [field: SerializeField] public InventoryButton[] InventoryArmorButtons { get; set; }
    [field: SerializeField] public InventoryButton[] InventoryArrowButtons { get; set; }

    
    private void OnEnable()
    {
        UpdateInventoryCells();
    }

    [BurstCompile]
    private void UpdateInventoryCells()
    {
       UpdateInventoryBag();
       UpdateWeaponSystem();
       UpdateArmorSystem();
       UpdateArrows();
       UpdateMoney();
    }

    [BurstCompile]
    public void UpdateInventoryBag()
    {
        ClearButtonsSet(InventoryBagButtons);

        var bag = InventoryUI.CurrentCharacter.Inventory.InventoryBag.GetCells();
        
        for (var i = 0; i < bag.Length - 1; i++)
        {
            var instance = bag[i];
            if (instance != null && !instance.IsEmpty())
            {
                InventoryBagButtons[i].SetItemData(instance.ItemData);
            }
        }
    }

    [BurstCompile]
    public void UpdateWeaponSystem()
    {
        ClearButtonsSet(InventoryWeaponButtons);

        for (var i = 0; i < InventoryUI.CurrentCharacter.Inventory.WeaponSystem.Instances.Length - 1; i++)
        {
            var instance = InventoryUI.CurrentCharacter.Inventory.WeaponSystem.Instances[i];
            if (instance != null && instance.ItemData != null)
            {
                InventoryWeaponButtons[i].SetItemData(instance.ItemData);
            }
        }
    }

    [BurstCompile]
    public void UpdateArmorSystem()
    {
        ClearButtonsSet(InventoryArmorButtons);
        Debug.Log($"Показываю ArmorSystem {InventoryUI.CurrentCharacter.name}");
    }

    [BurstCompile]
    public void UpdateArrows()
    {
        ClearButtonsSet(InventoryArrowButtons);
        Debug.Log($"Показываю Arrows {InventoryUI.CurrentCharacter.name}");
    }

    [BurstCompile]
    public void UpdateMoney()
    {
        Debug.Log($"Показываю Money {InventoryUI.CurrentCharacter.name}");
    }

    [BurstCompile]
    private void ClearButtonsSet(InventoryButton[] buttons)
    {
        foreach (var button in buttons)
        {
            button.SetItemData(null);
        }
    }
}
