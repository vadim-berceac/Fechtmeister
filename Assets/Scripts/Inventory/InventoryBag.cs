using UnityEngine;

public class InventoryBag
{
    public InventoryBag()
    {
        
    }

    public void AddItem(IItemData itemData)
    {
        Debug.Log($"{itemData.ItemName} Adding item to InventoryBag");
    }
}
