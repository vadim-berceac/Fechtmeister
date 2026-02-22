
using System;

public class BagCell : IInventoryCell
{
    public ISimpleItemData Data { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }
    public Action<int> OnQuantityChanged { get; set; }

    public BagCell(int maxQuantity)
    {
        MaxQuantity = maxQuantity;
    }
}
