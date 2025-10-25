using UnityEngine;
using Zenject;

public class WeaponDamageComponent : MonoBehaviour , IItemControlComponent
{
    public WeaponData ItemData { get; set; }
    public Collider Owner { get; set; }
    public bool IsAllowed { get; set; }
    private WeaponData _weaponData;
    private WeaponSystem _weaponSystem;
    private BoxCollider _weaponCollider;
    private Vector3 _originalColliderSize;
    private Vector3 _originalColliderCenter;
    private Rigidbody _rigidbody;
    
    // хранить параметры оружия
    
    [Inject] private SceneCharacterContainer _sceneCharacterContainer;

    [Inject]
    private void Construct()
    {
        //Debug.LogWarning("Constructed");
    }

    private void Awake()
    {
        if (!TryGetComponent<BoxCollider>(out _weaponCollider))
        {
            Debug.LogError($"{gameObject} doesn't have a Collider");
        }
        _weaponCollider.enabled = false;
        _originalColliderSize = _weaponCollider.size;
        _originalColliderCenter = _weaponCollider.center;
        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        
        var container = FindFirstObjectByType<SceneContext>()?.Container;
        container?.Inject(this);
    }

    public void SetWeaponSystem(WeaponSystem weaponSystem)
    {
        _weaponSystem = weaponSystem;
    }

    public void SetOwner(Collider coll)
    {
        Owner = coll;
    }

    public void SetData(IItemData data)
    {
        ItemData = (WeaponData)data;
    }
    
    public void AllowToUse(bool value)
    {
        IsAllowed = value;

        _weaponCollider.enabled = IsAllowed;

        if (IsAllowed)
        {
            SizeCollider(ItemData.WeaponParams.SizeModifer);
            return;
        }
        ResetCollider();
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
        target.Health.Damage(ItemData.WeaponParams.Damage);
        _weaponSystem.AllowAttack(false);
    }

    private void SizeCollider(float value)
    {
        _weaponCollider.center = new Vector3(_originalColliderCenter.x, _originalColliderCenter.y * value, _originalColliderCenter.z);
        _weaponCollider.size =  new Vector3(_originalColliderSize.x, _originalColliderSize.y * value, _originalColliderSize.z);
    }

    private void ResetCollider()
    {
        _weaponCollider.center = _originalColliderCenter;
        _weaponCollider.size = _originalColliderSize;
    }
}
