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

    public void AddItem(IEquppiedItemData equppiedItem)
    {
        var cellIndex = GetCorrectIndex(equppiedItem);

        if (cellIndex == -1)
        {
            Debug.Log("Bag is full");
            return;
        }
        _cells[cellIndex].AddItem(equppiedItem);
    }

    public void RemoveItem(IEquppiedItemData equppiedItem)
    {
        var cellIndex = _cells.IndexOf(_cells.FirstOrDefault(x => x.EquppiedItemData == equppiedItem));

        if (cellIndex == -1)
        {
            Debug.Log("Bag does not contain equppiedItem");
            return;
        }
        
        _cells[cellIndex].RemoveItem(1);
        //Debug.Log($"Removed equppiedItem {equppiedItem} ostalos {_cells[cellIndex].Quantity}");
    }

    private int GetCorrectIndex(IEquppiedItemData equppiedItem)
    {
        var index = GetSameItemData(equppiedItem);

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

    private int GetSameItemData(IEquppiedItemData equppiedItem)
    {
        return _cells.IndexOf(_cells.FirstOrDefault(x => x.EquppiedItemData == equppiedItem && !x.MaxQuantityReached()));
    }
    
    private int GetEmptyInstance()
    {
        return _cells.IndexOf(_cells.FirstOrDefault(x => x.IsEmpty()));
    }
}
