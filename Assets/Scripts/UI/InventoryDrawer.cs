using Unity.Burst;
using UnityEngine;

public class InventoryDrawer : MonoBehaviour
{
    [field: SerializeField] public InventoryUI InventoryUI { get; set; }
    
    [field: SerializeField] public InventoryButton[] InventoryBagButtons { get; set; }
    [field: SerializeField] public InventoryButton[] InventoryWeaponButtons { get; set; }
    [field: SerializeField] public InventoryButton[] InventoryArmorButtons { get; set; }
    [field: SerializeField] public InventoryButton[] InventoryProjectilesButtons { get; set; }

    
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
       UpdateProjectiles();
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
                InventoryBagButtons[i].SetItemData(instance.Data, instance.Quantity);
            }
        }
    }

    [BurstCompile]
    public void UpdateWeaponSystem()
    {
        ClearButtonsSet(InventoryWeaponButtons);

        for (var i = 0; i < InventoryUI.CurrentCharacter.Inventory.WeaponSystem.Instances.Length; i++)
        {
            var instance = InventoryUI.CurrentCharacter.Inventory.WeaponSystem.Instances[i];
            if (instance != null && instance.EquppiedItemData != null)
            {
                InventoryWeaponButtons[i].SetItemData(instance.EquppiedItemData, 0);
            }
        }
    }

    [BurstCompile]
    public void UpdateArmorSystem()
    {
        ClearButtonsSet(InventoryArmorButtons);
        
        for (var i = 0; i < InventoryUI.CurrentCharacter.Inventory.ArmorSystem.Instances.Length; i++)
        {
            var instance = InventoryUI.CurrentCharacter.Inventory.ArmorSystem.Instances[i];
            if (instance != null && instance.EquppiedItemData != null)
            {
                InventoryArmorButtons[i].SetItemData(instance.EquppiedItemData, 0);
            }
        }
    }

    [BurstCompile]
    public void UpdateProjectiles()
    {
        ClearButtonsSet(InventoryProjectilesButtons);
        var bag = InventoryUI.CurrentCharacter.Inventory.ProjectileSystem.GetCells();
        var instance = bag[0];
        InventoryProjectilesButtons[0].SetItemData(instance.Data,
            InventoryUI.CurrentCharacter.Inventory.ProjectileSystem.GetCell(instance.Data).Quantity);
        Debug.Log($"{instance.Quantity} {instance.Data == null}");
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
            button.SetItemData(null, 0);
        }
    }
}
