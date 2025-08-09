
public class BagCell : IInventoryCell
{
    public IItemData ItemData { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }

    public BagCell(int maxQuantity)
    {
        MaxQuantity = maxQuantity;
    }
}
