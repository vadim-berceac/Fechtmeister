
public interface IInventoryCell
{
    public IItemData ItemData { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }
}

public static class InventoryCellExtensions
{
    public static void AddItem(this IInventoryCell cell, IItemData item)
    {
        if (item == null)
        {
            cell.ItemData = null;
            cell.Quantity = 0;
            return;
        }

        if (item == cell.ItemData && cell.Quantity < cell.MaxQuantity)
        {
            cell.Quantity++;
            return;
        }
        cell.ItemData = item;
        cell.Quantity = 1;
    }

    public static void RemoveItem(this IInventoryCell cell, int quantity)
    {
        if (cell.ItemData == null)
        {
            return;
        }

        if (quantity > cell.MaxQuantity)
        {
            return;
        }
        
        cell.Quantity -= quantity;

        if (cell.Quantity <= 0)
        {
            cell.ItemData = null;
        }
    }
    
    public static bool IsEmpty(this IInventoryCell cell)
    {
        return cell.ItemData == null;
    }

    public static bool MaxQuantityReached(this IInventoryCell cell)
    {
        return cell.Quantity == cell.MaxQuantity;
    }

    public static void TransferItem(this IInventoryCell cell, IInventoryCell targetInventoryCell, int quantity)
    {
        
    }

    public static void ClearItems(this IInventoryCell cell)
    {
        
    }
}
