using System.Linq;
using ModestTree;
using UnityEngine;

public class InventoryBag : ICellContainer
{
    private readonly IInventoryCell[] _cells;
    
    public InventoryBag(int size)
    {
        _cells = new IInventoryCell[size];

        for (var i = 0; i < _cells.Length; i++)
        {
            _cells[i] = new BagCell(99);
        }
    }

    public IInventoryCell[] GetCells()
    {
        return _cells;
    }

    public void AddItem(IItemData item)
    {
        var cellIndex = GetCorrectIndex(item);

        if (cellIndex == -1)
        {
            Debug.Log("Bag is full");
            return;
        }
        _cells[cellIndex].AddItem(item);
    }

    public void RemoveItem(IItemData item)
    {
        var cellIndex = _cells.IndexOf(_cells.FirstOrDefault(x => x.ItemData == item));

        if (cellIndex == -1)
        {
            Debug.Log("Bag does not contain item");
            return;
        }
        
        _cells[cellIndex].RemoveItem(1);
    }

    private int GetCorrectIndex(IItemData item)
    {
        var index = GetSameItemData(item);

        if (index > -1)
        {
            return index;
        }
        
        index = GetEmptyInstance();

        if (index > -1)
        {
            return index;
        }
        
        return -1;
    }

    private int GetSameItemData(IItemData item)
    {
        return _cells.IndexOf(_cells.FirstOrDefault(x => x.ItemData == item && !x.MaxQuantityReached()));
    }
    
    private int GetEmptyInstance()
    {
        return _cells.IndexOf(_cells.FirstOrDefault(x => x.IsEmpty()));
    }
}
