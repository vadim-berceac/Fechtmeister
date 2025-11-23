
public class BagCell : IInventoryCell
{
    public IEquppiedItemData EquppiedItemData { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }

    public BagCell(int maxQuantity)
    {
        MaxQuantity = maxQuantity;
    }
}
