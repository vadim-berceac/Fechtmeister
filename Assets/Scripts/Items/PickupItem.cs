using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    [SerializeField] private ScriptableObject itemData; 

    public IItemData ItemData
    {
        get => itemData as IItemData; 
        set => itemData = value as ScriptableObject; 
    }

    private void Start()
    {
        SetLayer(24);
    }

    private void SetLayer(int layer)
    {
        if (ItemData == null)
        {
            return;
        }
        gameObject.layer = 24;
    }
}
