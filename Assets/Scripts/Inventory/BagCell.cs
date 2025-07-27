
using UnityEngine;

public class BagCell : IInventoryCell
{
    public IItemData ItemData { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }

    public BagCell()
    {
        
    }

    public bool IsEmpty()
    {
        return ItemData == null || Quantity == 0;
    }

    public bool MaxQuantityReached()
    {
        return false;
    }

    public void AddItem(IItemData item)
    {
        Debug.Log($"{item.ItemName} Adding item to InventoryBag");
    }

    public void RemoveItem(int quantity)
    {
        
    }

    public void TransferItem(IInventoryCell inventoryCell, int quantity)
    {
        
    }

    public void ClearItems()
    {
        
    }
}
