using UnityEngine;
using Zenject;

public class WeaponDamageComponent : MonoBehaviour , IItemControlComponent
{
    public WeaponData ItemData { get; set; }
    public Collider Owner { get; set; }
    public bool IsAllowed { get; set; }
    private WeaponData _weaponData;
    private BoxCollider _weaponCollider;
    private Rigidbody _rigidbody;
    private StateTimer _stateTimer;
    
    // хранить параметры оружия
    
    [Inject] private SceneCharacterContainer _sceneCharacterContainer;

    [Inject]
    private void Construct()
    {
        //Debug.LogWarning("Constructed");
    }

    private void Awake()
    {
        if (!TryGetComponent(out _weaponCollider))
        {
            Debug.LogError($"{gameObject} doesn't have a Collider");
        }
        _weaponCollider.enabled = false;
        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        
        var container = FindFirstObjectByType<SceneContext>()?.Container;
        container?.Inject(this);
    }

    public void SetStateTimer(StateTimer stateTimer)
    {
        _stateTimer = stateTimer;
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
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsAllowed || !_stateTimer.ActionIsPossible())
        {
            return;
        }

        if (other == Owner)
        {
            return;
        }

        var target = _sceneCharacterContainer.GetCharacter(other);
       
        if (target == null)
        {
            return;
        }
        
        AllowToUse(false);
        _stateTimer.SetActionIsPossible(false);
        target.Health.Damage(ItemData.WeaponParams.Damage);
    }
}
