using UnityEngine;
using Zenject;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    [field: SerializeField] private GameObject namePlatePrefab;
    
    [field: SerializeField] private ScriptableObject itemData; 
    [field: SerializeField] private int amount = 1;
    
    private NameUI _namePlate;
    private DiContainer _container;
    

    [Inject]
    private void Construct(DiContainer container)
    {
        _container = container;
    }
    
    public (ISimpleItemData data, int amount) GetItemData()
    {
        return (itemData as ISimpleItemData, amount);
    }

    private void Start()
    {
        SetLayer();
        CreateNamePlate();
        ShowNamePlate(false);
    }

    private void SetLayer()
    {
        if (itemData == null)
        {
            return;
        }
        gameObject.layer = 24;
    }

    private void CreateNamePlate()
    {
        if (namePlatePrefab == null)
        {
            return;
        }
        
        _namePlate = Instantiate(namePlatePrefab, transform).GetComponent<NameUI>();
        _container.InjectGameObject(_namePlate.gameObject);
        _namePlate.Set(GetItemData().data);
    }

    public void ShowNamePlate(bool show)
    {
        if (_namePlate == null)
        {
            return;
        }
        _namePlate.gameObject.SetActive(show);
    }
}
