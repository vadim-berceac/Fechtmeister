
public interface ICellContainer
{
    public IInventoryCell[] GetCells();
    public void AddItem(IEquppiedItemData equppiedItem);
    public void RemoveItem(IEquppiedItemData equppiedItem);
}
