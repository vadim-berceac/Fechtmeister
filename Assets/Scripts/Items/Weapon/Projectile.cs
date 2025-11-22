using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [field: Header("Damage Settings")]
    [field: SerializeField, Range (0f, 15f)] private float additionalDamageValue;
    [field: SerializeField] private float stickOffset = 0.05f;
    [field: SerializeField] private DamageTypes damageType;

    [field: Header("Physics Settings")]
    [field: SerializeField] private float gravity = -9.81f;
    [field: SerializeField] private float drag = 0.5f;
    [field: SerializeField] private float speed = 100f;
    [field: SerializeField, Range (0.01f, 0.5f)] private float maxSpreadRadius = 0.1f;

    [field: Header("Collision Settings")]
    [field: SerializeField] private float hitRadius = 0.2f;
    [field: SerializeField] private float lifetime = 5f;
    [field: SerializeField] private LayerMask layerMask;
    
    [field:Header("Spawn Settings")]
    [field: SerializeField] public Vector3 StartPositionOffset {get; private set;}

    private Vector3 _velocity;
    private Collider _parent;
    private Transform _transform;
    private readonly Collider[] _hitResults = new Collider[5];
    private bool _stuck;

    private NativeArray<Vector3> _velArray;
    private NativeArray<Vector3> _posArray;

    private void Start()
    {
        _transform = transform;
        Destroy(gameObject, lifetime);

        _velArray = new NativeArray<Vector3>(1, Allocator.Persistent);
        _posArray = new NativeArray<Vector3>(1, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (_velArray.IsCreated) _velArray.Dispose();
        if (_posArray.IsCreated) _posArray.Dispose();
    }

    public void Launch(Collider parent, Vector3 direction, int accuracy)
    {
        _parent = parent;
        
        var spreadRadius = maxSpreadRadius * (1f - accuracy / 100f);
       
        var randomOffset = Random.insideUnitCircle * spreadRadius;
        var right = Vector3.Cross(direction, Vector3.up).normalized;
        var up = Vector3.Cross(right, direction).normalized;

        var spread = right * randomOffset.x + up * randomOffset.y;
        
        var finalDirection = (direction + spread).normalized;

        _velocity = finalDirection * speed;
    }


    private void FixedUpdate()
    {
        if (_stuck || _velocity.magnitude < 0.1f) return;

        RunPhysicsJob();
        UpdateRotation();
        DetectCollisions();
    }
    
    private void RunPhysicsJob()
    {
        var dt = Time.fixedDeltaTime;
        
        _velArray[0] = _velocity;
        _posArray[0] = _transform.position;

        var job = new ProjectileJob
        {
            velocity = _velArray,
            position = _posArray,
            gravity = gravity,
            drag = drag,
            deltaTime = dt
        };

        var handle = job.Schedule();
        handle.Complete();
        
        _velocity = _velArray[0];
        _transform.position = _posArray[0];
    }
   
    private void UpdateRotation()
    {
        if (_velocity.magnitude > 0.1f)
        {
            _transform.rotation = Quaternion.LookRotation(_velocity);
        }
    }
    
    private void DetectCollisions()
    {
        var hitCount = Physics.OverlapSphereNonAlloc(_transform.position, hitRadius, _hitResults, layerMask);
        for (var i = 0; i < hitCount; i++)
        {
            var hit = _hitResults[i];
            if (hit == null || hit.isTrigger || hit == _parent) continue;

            HandleHit(hit);
            return;
        }
    }
    
    private void HandleHit(Collider hit)
    {
        Debug.Log($"Попадание в {hit.name} на слое {LayerMask.LayerToName(hit.gameObject.layer)}");

        var damaged = hit.GetComponent<IDamageable>();
        var hitDirection = _velocity.normalized;

        _velocity = Vector3.zero;
        _stuck = true;
        
        var closest = hit.ClosestPoint(_transform.position);
        _transform.position = closest - hitDirection * stickOffset;

        if (damaged != null)
        {
            damaged.Damage(additionalDamageValue, damageType);

            if (damaged.IsHitReactionEnabled)
            {
                _transform.SetParent(damaged.DamagedObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            _transform.SetParent(hit.transform);
        }
    }

   
    [BurstCompile]
    private struct ProjectileJob : IJob
    {
        public NativeArray<Vector3> velocity;
        public NativeArray<Vector3> position;

        public float gravity;
        public float drag;
        public float deltaTime;

        public void Execute()
        {
            var vel = velocity[0];
            var pos = position[0];

            vel += gravity * deltaTime * Vector3.up;
            vel *= Mathf.Exp(-drag * deltaTime);
            pos += vel * deltaTime;

            velocity[0] = vel;
            position[0] = pos;
        }
    }
}
