using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

public class CharacterSpring : ManagedUpdatableObject
{
    private Transform _character;
    private Transform _deformationBody;
    [SerializeField] private ConfigurableJoint configurableJoint;
    [SerializeField] private Vector3 upScale = new (0.8f, 1.2f, 0.8f);
    [SerializeField] private Vector3 downScale = new (1.2f, 0.8f, 1.2f);

    [SerializeField] private float scaleFactor = 1f;
    [SerializeField] private float rotationFactor = 1f;
    
    private Transform _springTransform;
    private JobHandle _jobHandle;
    
    private NativeArray<float3> _resultScale;
    private NativeArray<float3> _resultRotation;

    public void Initialize(Transform character, Transform deformationBody, Rigidbody rb)
    {
        _springTransform = transform;
        _character = character;
        _deformationBody = deformationBody;
        configurableJoint.connectedBody = rb;
        
        name = _character.name + "_Spring";
        
        _resultScale = new NativeArray<float3>(1, Allocator.Persistent);
        _resultRotation = new NativeArray<float3>(1, Allocator.Persistent);
    }

    public override void OnManagedUpdate()
    {
        
    }

    public override void OnManagedFixedUpdate()
    {
        _jobHandle.Complete();
       
        float3 springWorldPos = _springTransform.position;
        float3 characterWorldPos = _character.position;
        quaternion characterRotation = _character.rotation;
      
        var job = new DeformationCalculationJob
        {
            springWorldPosition = springWorldPos,
            characterWorldPosition = characterWorldPos,
            characterRotation = characterRotation,
            upScale = upScale,
            downScale = downScale,
            scaleFactor = scaleFactor,
            rotationFactor = rotationFactor,
            resultScale = _resultScale,
            resultRotation = _resultRotation
        };
        
        _jobHandle = job.Schedule();
        
        _jobHandle.Complete();
      
        _deformationBody.localScale = _resultScale[0];
        _deformationBody.localEulerAngles = _resultRotation[0];
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        _jobHandle.Complete();
        
        if (_resultScale.IsCreated)
            _resultScale.Dispose();
        if (_resultRotation.IsCreated)
            _resultRotation.Dispose();
    }

    [BurstCompile]
    private struct DeformationCalculationJob : IJob
    {
        [ReadOnly] public float3 springWorldPosition;
        [ReadOnly] public float3 characterWorldPosition;
        [ReadOnly] public quaternion characterRotation;
        [ReadOnly] public float3 upScale;
        [ReadOnly] public float3 downScale;
        [ReadOnly] public float scaleFactor;
        [ReadOnly] public float rotationFactor;
        
        public NativeArray<float3> resultScale;
        public NativeArray<float3> resultRotation;

        public void Execute()
        {
            var relativePosition = math.mul(math.inverse(characterRotation), 
                springWorldPosition - characterWorldPosition);
            
            var interpolant = relativePosition.y * scaleFactor;
            var currentScale = Lerp3(downScale, new float3(1, 1, 1), upScale, interpolant);
            
            var rotation = new float3(relativePosition.z, 0, -relativePosition.x) * rotationFactor;
            
            resultScale[0] = currentScale;
            resultRotation[0] = math.degrees(rotation);
        }

        private static float3 Lerp3(float3 a, float3 b, float3 c, float t)
        {
            return t < 0 
                ? math.lerp(a, b, t + 1) 
                : math.lerp(b, c, t);
        }
    }
}