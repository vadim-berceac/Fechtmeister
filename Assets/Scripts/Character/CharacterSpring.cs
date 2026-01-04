using UnityEngine;

public class CharacterSpring : MonoBehaviour
{
    private Transform _character;
    private Transform _deformationBody;
    [SerializeField] private ConfigurableJoint configurableJoint;
    [SerializeField] private Vector3 upScale = new (0.8f, 1.2f, 0.8f);
    [SerializeField] private Vector3 downScale = new (1.2f, 0.8f, 1.2f);

    [SerializeField] private float scaleFactor = 1f;
    [SerializeField] private float rotationFactor = 1f;
    
    private Transform _springTransform;
    private Vector3 _relativePosition;
    private Vector3 _currentScale;
    private float _interpolant;

    public void Initialize(Transform character, Transform deformationBody, Rigidbody rb)
    {
        _springTransform = transform;
        _character = character;
        _deformationBody = deformationBody;
        configurableJoint.connectedBody = rb;
        
        name = _character.name + "_Spring";
    }

    private void Update()
    {
        _relativePosition = _character.InverseTransformPoint(_springTransform.position);
        _interpolant = _relativePosition.y * scaleFactor;
        _currentScale = Lerp3(downScale, Vector3.one, upScale, _interpolant);
        _deformationBody.localScale = _currentScale;
        
        _deformationBody.localEulerAngles = new Vector3(_relativePosition.z, 0, - _relativePosition.x) * rotationFactor;
    }

    private static Vector3 Lerp3(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return t < 0 ? Vector3.LerpUnclamped(a, b, t + 1) : Vector3.LerpUnclamped(b, c, t);
    }
}
