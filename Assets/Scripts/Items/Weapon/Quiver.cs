using UnityEngine;

public class Quiver : MonoBehaviour
{
    [SerializeField] private Transform[] arrows;
    private ProjectileSystem _projectileSystem;
    private IInventoryCell _cell;

    private void Start()
    {
        _projectileSystem = gameObject.GetComponentInParent<CharacterCore>().Inventory.ProjectileSystem;
        var instance = _projectileSystem.Instances[0].EquppiedItemData;
        _cell = _projectileSystem.GetCell(instance);

        UpdateArrows(_cell.Quantity);
        _cell.OnQuantityChanged += UpdateArrows;
    }

    private void UpdateArrows(int quantity)
    {
        var activeCount = Mathf.Min(quantity, arrows.Length);

        for (var i = 0; i < arrows.Length; i++)
        {
            arrows[i].gameObject.SetActive(i < activeCount);
        }
        Debug.LogWarning(quantity);
    }

    private void OnDestroy()
    {
        if (_cell != null)
            _cell.OnQuantityChanged -= UpdateArrows;
    }
}