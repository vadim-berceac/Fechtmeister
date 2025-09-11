using UnityEngine;
using Zenject;

public class WeaponDamageComponent : MonoBehaviour
{
    private bool _allowToDamage;
    private Collider _weaponCollider;
    private Collider _ownerCollider;
    private Rigidbody _rigidbody;
    
    [Inject] private SceneCharacterContainer _sceneCharacterContainer;

    [Inject]
    private void Construct()
    {
        Debug.LogWarning("Constructed");
    }

    private void Awake()
    {
        if (!TryGetComponent<Collider>(out _weaponCollider))
        {
            Debug.LogError($"{gameObject} doesn't have a Collider");
        }
        
        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        
        var container = FindFirstObjectByType<SceneContext>()?.Container;
        if (container == null)
        {
            return;
        }
        container.Inject(this);
        
        //временно
        AllowToDamage(true);
    }

    public void SetOwnerCollider(Collider coll)
    {
        _ownerCollider = coll;
    }

    public void AllowToDamage(bool value)
    {
        _allowToDamage = value;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!_allowToDamage)
        {
            return;
        }

        if (other == _ownerCollider)
        {
            return;
        }
        
        //одна попытка нанести урон - после этого в текущем состоянии ее отключаем

        var target = _sceneCharacterContainer.GetCharacter(other);
        if (target == null)
        {
            return;
        }
        Debug.Log($"{gameObject} collided with {target.gameObject.name}");
    }
}
