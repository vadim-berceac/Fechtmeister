
using System;

public interface IInventoryCell
{
    public ISimpleItemData Data { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }
}

public static class InventoryCellExtensions
{
    public static void AddItem(this IInventoryCell cell, ISimpleItemData data, int amount)
    {
        if (cell == null)
        {
            throw new ArgumentNullException(nameof(cell));
        }

        if (data == null)
        {
            cell.Data = null;
            cell.Quantity = 0;
            return;
        }

        if (amount <= 0)
        {
            return; 
        }

        var targetQuantity = cell.Quantity + amount;

        if (data == cell.Data)
        {
            cell.Quantity = Math.Min(targetQuantity, cell.MaxQuantity);
            return;
        }
        
        cell.Data = data;
        cell.Quantity = Math.Min(amount, cell.MaxQuantity);
    }

    public static void RemoveItem(this IInventoryCell cell, int quantity)
    {
        if (cell == null)
        {
            throw new ArgumentNullException(nameof(cell));
        }
        
        if (cell.Data == null || cell.Quantity <= 0)
        {
            return;
        }
        
        if (quantity <= 0)
        {
            return; 
        }

        var amountToRemove = Math.Min(quantity, cell.Quantity);
        cell.Quantity -= amountToRemove;
        
        if (cell.Quantity <= 0)
        {
            cell.Data = null;
            cell.Quantity = 0; 
        }
    }
    
    public static bool IsEmpty(this IInventoryCell cell)
    {
        return cell.Data == null;
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
