
public interface IInventoryCell
{
    public IEquppiedItemData EquppiedItemData { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }
}

public static class InventoryCellExtensions
{
    public static void AddItem(this IInventoryCell cell, IEquppiedItemData equppiedItem)
    {
        if (equppiedItem == null)
        {
            cell.EquppiedItemData = null;
            cell.Quantity = 0;
            return;
        }

        if (equppiedItem == cell.EquppiedItemData && cell.Quantity < cell.MaxQuantity)
        {
            cell.Quantity++;
            return;
        }
        cell.EquppiedItemData = equppiedItem;
        cell.Quantity = 1;
    }

    public static void RemoveItem(this IInventoryCell cell, int quantity)
    {
        if (cell.EquppiedItemData == null)
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
            cell.EquppiedItemData = null;
        }
    }
    
    public static bool IsEmpty(this IInventoryCell cell)
    {
        return cell.EquppiedItemData == null;
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
