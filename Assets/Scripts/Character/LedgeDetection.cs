using UnityEngine;

public class LedgeDetection
{
    private readonly Transform _sphereCastOrigin; 
    private readonly float _sphereRadius; 
    private readonly float _castDistance; 
    private readonly Vector3 _offset;
    private readonly LayerMask _ledgeLayerMask;
    private Vector3 _lastHitNormal;
    
    public Vector3 LedgeGrabPoint { get; private set; }
    public Vector3 LastLedgeNormal { get; private set; }
    public Vector3 LastLedgeGrabPoint { get; private set; }

    public LedgeDetection(LedgeDetectionSettings ledgeDetectionSettings)
    {
        _sphereCastOrigin = ledgeDetectionSettings.SphereCastOrigin;
        _sphereRadius = ledgeDetectionSettings.SphereRadius;
        _castDistance = ledgeDetectionSettings.CastDistance;
        _offset = ledgeDetectionSettings.OffsetOnEnd;
        _ledgeLayerMask = ledgeDetectionSettings.LayerMask;
    }

    public void Reset()
    {
        LedgeGrabPoint = Vector3.zero;
        LastLedgeGrabPoint = Vector3.zero;
    }
    
    public void UpdateDetection(bool value)
    {
        if (!value)
        {
            if (LedgeGrabPoint == Vector3.zero)
            {
                return;
            }

            // Сохраняем точку и нормаль стены
            LastLedgeGrabPoint = LedgeGrabPoint + _sphereCastOrigin.TransformDirection(_offset);
            LastLedgeNormal = _lastHitNormal; // ← обновляем нормаль из последнего SphereCast
            LedgeGrabPoint = Vector3.zero;
        }

        LedgeGrabPoint = DetectLedge();
    }

    
    private Vector3 DetectLedge()
    {
        // Шаг 1: SphereCast вперёд для обнаружения стены
        if (!Physics.SphereCast(_sphereCastOrigin.position, _sphereRadius, _sphereCastOrigin.forward,
                out var hit, _castDistance, _ledgeLayerMask))
        {
            return Vector3.zero;
        }

        _lastHitNormal = hit.normal;


        // Шаг 2: Проверка верхней поверхности уступа — луч вниз
        var ledgeCheckOrigin = hit.point + Vector3.up * 1.0f;
        if (!Physics.Raycast(ledgeCheckOrigin, Vector3.down, out var ledgeHit, 2.0f, _ledgeLayerMask))
        {
            return Vector3.zero;
        }

        // Шаг 3: Проверка, что поверхность горизонтальна
        var surfaceAngle = Vector3.Angle(ledgeHit.normal, Vector3.up);
        if (surfaceAngle > 30f)
        {
            return Vector3.zero;
        }

        // Шаг 4: Проверка свободного пространства над уступом
        var isSpaceAbove = !Physics.CheckSphere(ledgeHit.point + Vector3.up * 1.0f, 0.5f, _ledgeLayerMask);
        if (!isSpaceAbove)
        {
            return Vector3.zero;
        }

        // Шаг 5: Возврат точки захвата с небольшой корректировкой 
        return ledgeHit.point + Vector3.forward;
    }
}

[System.Serializable]
public struct LedgeDetectionSettings
{
    [field: SerializeField] public Transform SphereCastOrigin { get; set; }
    [field: SerializeField] public float SphereRadius { get; set; }
    [field: SerializeField] public float CastDistance { get; set; }
    [field: SerializeField] public Vector3 OffsetOnEnd { get; set; }
    [field: SerializeField] public LayerMask LayerMask { get; set; }
}
