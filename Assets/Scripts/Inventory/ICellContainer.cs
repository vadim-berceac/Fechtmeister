
public interface ICellContainer
{
    public IInventoryCell[] GetCells();
    public IInventoryCell GetCell(ISimpleItemData data);
    public void AddItem(ISimpleItemData data, int amount);
    public void RemoveItem(ISimpleItemData data, int amount);
}
