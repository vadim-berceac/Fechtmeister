
using UnityEngine;

public class BagCell : IInventoryCell
{
    public IItemData ItemData { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }

    public BagCell(int maxQuantity)
    {
        MaxQuantity = maxQuantity;
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
        if (item == null)
        {
            Debug.Log("Item is null");
            ItemData = null;
            Quantity = 0;
            return;
        }

        if (item == ItemData && Quantity < MaxQuantity)
        {
            Debug.Log("Item is already in the bag");
            Quantity++;
            return;
        }
        Debug.Log("DWdwdqe");
        ItemData = item;
        Quantity = 1;
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
