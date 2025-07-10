using UnityEngine;

public class SpineProxy : MonoBehaviour
{
    [SerializeField] private Transform originalSpine;
    private bool _isAllowed;
    private Transform _cashedTransform;

    private Quaternion _rotationOffset = Quaternion.identity;
        
    private void Awake()
    {
        _cashedTransform = transform;
        
        if (originalSpine != null)
        {
            _rotationOffset = Quaternion.Inverse(_cashedTransform.rotation) * originalSpine.rotation;
        }
    }
   
    private void LateUpdate()
    {
        if (!_isAllowed || originalSpine == null)
        {
            return;
        }
        originalSpine.rotation = _cashedTransform.rotation * _rotationOffset;
    }

    public void Allow(bool value)
    {
        _isAllowed = value;
    }
}
