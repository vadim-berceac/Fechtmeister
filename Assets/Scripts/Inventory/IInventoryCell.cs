using System;

public interface IInventoryCell
{
    public ISimpleItemData Data { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }
    public Action<int> OnQuantityChanged { get; set; }
}

public static class InventoryCellExtensions
{
    public static void AddItem(this IInventoryCell cell, ISimpleItemData data, int amount)
    {
        if (cell == null)
            throw new ArgumentNullException(nameof(cell));

        if (data == null)
        {
            if (cell.Quantity != 0 || cell.Data != null)
            {
                cell.Data = null;
                cell.Quantity = 0;
                cell.OnQuantityChanged?.Invoke(cell.Quantity);
            }
            return;
        }

        if (amount <= 0)
            return;

        if (cell.MaxQuantity <= 0)
        {
            throw new InvalidOperationException(
                $"Cell has invalid MaxQuantity ({cell.MaxQuantity}). Must be greater than zero.");
        }
        
        if (!cell.IsEmpty() && data != cell.Data)
        {
            throw new InvalidOperationException(
                $"Cell already contains a different item. Clear the cell before adding a new one.");
        }

        var previousQuantity = cell.Quantity;

        cell.Data = data;
        cell.Quantity = Math.Min(cell.Quantity + amount, cell.MaxQuantity);

        if (cell.Quantity != previousQuantity)
            cell.OnQuantityChanged?.Invoke(cell.Quantity);
    }

    public static void RemoveItem(this IInventoryCell cell, int quantity)
    {
        if (cell == null)
            throw new ArgumentNullException(nameof(cell));

        if (cell.IsEmpty() || quantity <= 0)
            return;

        var amountToRemove = Math.Min(quantity, cell.Quantity);
        cell.Quantity -= amountToRemove;

        if (cell.Quantity <= 0)
        {
            cell.Data = null;
            cell.Quantity = 0;
        }

        cell.OnQuantityChanged?.Invoke(cell.Quantity);
    }

    public static void TransferItem(this IInventoryCell cell, IInventoryCell targetCell, int quantity)
    {
        if (cell == null)
            throw new ArgumentNullException(nameof(cell));

        if (targetCell == null)
            throw new ArgumentNullException(nameof(targetCell));

        if (cell.IsEmpty() || quantity <= 0)
            return;

        if (!targetCell.IsEmpty() && targetCell.Data != cell.Data)
            return;

        var availableSpace = targetCell.MaxQuantity - targetCell.Quantity;
        var actualTransfer = Math.Min(Math.Min(quantity, cell.Quantity), availableSpace);

        if (actualTransfer <= 0)
            return;

        targetCell.AddItem(cell.Data, actualTransfer);
        cell.RemoveItem(actualTransfer);
    }

    public static void ClearItems(this IInventoryCell cell)
    {
        if (cell == null)
            throw new ArgumentNullException(nameof(cell));

        if (cell.IsEmpty())
            return;

        cell.Data = null;
        cell.Quantity = 0;
        cell.OnQuantityChanged?.Invoke(cell.Quantity);
    }
    
    public static bool IsEmpty(this IInventoryCell cell)
    {
        return cell.Data == null || cell.Quantity <= 0;
    }

    public static bool MaxQuantityReached(this IInventoryCell cell)
    {
        return cell.Quantity >= cell.MaxQuantity;
    }
}