using UnityEngine;

public class WeaponDamageComponent : MonoBehaviour
{
    private bool _allowToDamage;
    private Collider _weaponCollider;

    private void Awake()
    {
        if (!TryGetComponent<Collider>(out _weaponCollider))
        {
            Debug.LogError($"{gameObject} don't have a Collider");
        }
    }

    public void AllowToDamage(bool value)
    {
        _allowToDamage = value;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!_allowToDamage)
        {
            return;
        }
        
        //одна попытка нанести урон - после этого в текущем состоянии ее отключаем
    }
}
