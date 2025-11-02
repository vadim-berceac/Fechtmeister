using UnityEngine;

public class LedgeDetection
{
    private readonly Transform _sphereCastOrigin0; 
    private readonly Transform _sphereCastOrigin1; 
    private readonly Transform _sphereCastOrigin2; 
    private readonly float _sphereRadius; 
    private readonly float _castDistance;
    private readonly LayerMask _ledgeLayerMask;
    private Vector3 _lastHitNormal;
    
    public Vector3 LedgeGrabPoint { get; private set; }
    public Vector3 LastWallNormal { get; private set; } 
    public Vector3 LastLedgeGrabPoint { get; private set; }
    public int LedgeType { get; private set; }

    public LedgeDetection(LedgeDetectionSettings ledgeDetectionSettings)
    {
        _sphereCastOrigin0 = ledgeDetectionSettings.SphereCastOrigin0;
        _sphereCastOrigin1 = ledgeDetectionSettings.SphereCastOrigin1;
        _sphereCastOrigin2 = ledgeDetectionSettings.SphereCastOrigin2;
        _sphereRadius = ledgeDetectionSettings.SphereRadius;
        _castDistance = ledgeDetectionSettings.CastDistance;
        _ledgeLayerMask = ledgeDetectionSettings.LayerMask;
    }

    public void Reset()
    {
        LedgeGrabPoint = Vector3.zero;
        LastLedgeGrabPoint = Vector3.zero;
        LastWallNormal = Vector3.zero; 
    }
    
    public void UpdateDetection(bool enabled, LedgeTypeDetection ledgeType)
    {
        if (!enabled)
        {
            if (LedgeGrabPoint == Vector3.zero)
            {
                return;
            }
            LastLedgeGrabPoint = LedgeGrabPoint;
            LastWallNormal = _lastHitNormal; 
            LedgeGrabPoint = Vector3.zero;
        }

        LedgeGrabPoint = DetectLedge(GetDetectionOrigin(ledgeType));
        LedgeType = (int)ledgeType;
    }

    
    private Vector3 DetectLedge(Transform origin)
    {
        if (!Physics.SphereCast(origin.position, _sphereRadius, origin.forward,
                out var hit, _castDistance, _ledgeLayerMask))
        {
            return Vector3.zero;
        }

        _lastHitNormal = hit.normal;  
        
        var ledgeCheckOrigin = hit.point + Vector3.up * 1.0f;
        if (!Physics.Raycast(ledgeCheckOrigin, Vector3.down, out var ledgeHit, 2.0f, _ledgeLayerMask))
        {
            return Vector3.zero;
        }
        
        var surfaceAngle = Vector3.Angle(ledgeHit.normal, Vector3.up);
        if (surfaceAngle > 30f)
        {
            return Vector3.zero;
        }
        
        var isSpaceAbove = !Physics.CheckSphere(ledgeHit.point + Vector3.up * 1.0f, 0.5f, _ledgeLayerMask);
        if (!isSpaceAbove)
        {
            return Vector3.zero;
        }

        var grabPoint = ledgeHit.point;

        LastWallNormal = hit.normal; 

        return grabPoint;
    }

    private Transform GetDetectionOrigin(LedgeTypeDetection ledgeType)
    {
        switch (ledgeType)
        {
            case LedgeTypeDetection.High:
                return _sphereCastOrigin0;
            
            case LedgeTypeDetection.Middle:
                return _sphereCastOrigin1;
            
            case LedgeTypeDetection.Low:
                return _sphereCastOrigin2;
            
            default: return null;
        }
    }
}

public enum LedgeTypeDetection
{
    High = 0,
    Middle = 1,
    Low = 2
}

[System.Serializable]
public struct LedgeDetectionSettings
{
    [field: SerializeField] public Transform SphereCastOrigin0 { get; set; }
    [field: SerializeField] public Transform SphereCastOrigin1 { get; set; }
    [field: SerializeField] public Transform SphereCastOrigin2 { get; set; }
    [field: SerializeField] public float SphereRadius { get; set; }
    [field: SerializeField] public float CastDistance { get; set; }
    [field: SerializeField] public LayerMask LayerMask { get; set; }
}