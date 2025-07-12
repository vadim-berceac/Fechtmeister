
public interface IInventoryCell
{
    public IItemData ItemData { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }

    public bool IsEmpty();
    public bool MaxQuantityReached();
    public void AddItem(IItemData item);
    public void RemoveItem(int quantity);
    public void TransferItem(IInventoryCell inventoryCell, int quantity);
    public void ClearItems();
}
