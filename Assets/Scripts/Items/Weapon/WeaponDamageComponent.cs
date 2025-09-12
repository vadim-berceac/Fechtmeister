using UnityEngine;
using Zenject;

public class WeaponDamageComponent : MonoBehaviour , IItemControlComponent
{
    public Collider Owner { get; set; }
    public bool IsAllowed { get; set; }
    private Collider _weaponCollider;
    private Rigidbody _rigidbody;
    
    // хранить параметры оружия
    
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
        AllowToUse(true);
    }

    public void SetOwner(Collider coll)
    {
        Owner = coll;
    }
    
    public void AllowToUse(bool value)
    {
        IsAllowed = value;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsAllowed)
        {
            return;
        }

        if (other == Owner)
        {
            return;
        }
        
        //одна попытка нанести урон - после этого в текущем состоянии ее отключаем

        var target = _sceneCharacterContainer.GetCharacter(other);
       
        if (target == null)
        {
            return;
        }
        
        AllowToUse(false);
        
        //временно
        target.Health.Damage(40);
        
    }
}
