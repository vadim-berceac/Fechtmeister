using UnityEngine;
using Zenject;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    [field: SerializeField] private GameObject namePlatePrefab;
    
    [SerializeField] private ScriptableObject itemData; 
    
    private NameUI _namePlate;
    private DiContainer _container;

    [Inject]
    private void Construct(DiContainer container)
    {
        _container = container;
    }

    public IItemData ItemData
    {
        get => itemData as IItemData; 
        set => itemData = value as ScriptableObject; 
    }

    private void Start()
    {
        SetLayer(24);
        CreateNamePlate();
        ShowNamePlate(false);
    }

    private void SetLayer(int layer)
    {
        if (ItemData == null)
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
        _namePlate.Set(ItemData);
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
