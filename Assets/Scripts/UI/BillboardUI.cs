using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Zenject;

public class BillboardUI : ManagedUpdatableObject
{
    private Transform _cameraTransform;
    private Transform _cashedTransform;
    
    private NativeArray<Vector3> _positions;
    private NativeArray<Quaternion> _rotations;
    private JobHandle _jobHandle;

    [Inject]
    private void Construct(SceneCamera sceneCamera)
    {
        _cameraTransform = sceneCamera.SceneCameraData.CharacterCamera;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _cashedTransform = transform;
        
        _positions = new NativeArray<Vector3>(2, Allocator.Persistent);
        _rotations = new NativeArray<Quaternion>(1, Allocator.Persistent);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
       
        _jobHandle.Complete();
       
        if (_positions.IsCreated)
            _positions.Dispose();
        if (_rotations.IsCreated)
            _rotations.Dispose();
    }

    public override void OnManagedUpdate()
    {
        
    }
    
    public override void OnManagedLateUpdate()
    {
        FollowCamera();
    }

    private void FollowCamera()
    {
        _jobHandle.Complete();
       
        _positions[0] = _cashedTransform.position;
        _positions[1] = _cameraTransform.position;
      
        var job = new BillboardRotationJob
        {
            positions = _positions,
            cameraRotation = _cameraTransform.rotation,
            result = _rotations
        };
        
        _jobHandle = job.Schedule();
        _jobHandle.Complete(); 
       
        _cashedTransform.rotation = _rotations[0];
    }
}

[BurstCompile]
struct BillboardRotationJob : IJob
{
    [ReadOnly] public NativeArray<Vector3> positions;
    [ReadOnly] public Quaternion cameraRotation;
    
    public NativeArray<Quaternion> result;
    
    public void Execute()
    {
        Vector3 billboardPos = positions[0];
        Vector3 cameraForward = cameraRotation * Vector3.forward;
        Vector3 cameraUp = cameraRotation * Vector3.up;
        
        Vector3 lookAtPosition = billboardPos + cameraForward;
        Vector3 direction = lookAtPosition - billboardPos;
        
        if (direction.sqrMagnitude > 0.001f)
        {
            result[0] = Quaternion.LookRotation(direction, cameraUp);
        }
    }
}