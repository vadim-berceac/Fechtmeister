
public interface ICellContainer
{
    public IInventoryCell[] GetCells();
    public void AddItem(IItemData item);
    public void RemoveItem(IItemData item);
}
