using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private ProjectileData _data;
    private WeaponData _weaponData;
    private Vector3 _velocity;
    private Collider _parent;
    private HealthComponent _parentHealth;
    private Transform _transform;
    private readonly Collider[] _hitResults = new Collider[5];
    private readonly Collider[] _threatResults = new Collider[10]; 
    private bool _stuck;

    private NativeArray<Vector3> _velArray;
    private NativeArray<Vector3> _posArray;

    
    private const float ThreatDetectionRadius = 10f; 
    private LayerMask _characterLayerMask; 
    
    private bool _threatNotified; 

    private void Start()
    {
        _transform = transform;
        _velArray = new NativeArray<Vector3>(1, Allocator.Persistent);
        _posArray = new NativeArray<Vector3>(1, Allocator.Persistent);
      
        if (_characterLayerMask == 0)
        {
            _characterLayerMask = LayerMask.GetMask("Character");
        }
    }

    private void OnDestroy()
    {
        if (_velArray.IsCreated) _velArray.Dispose();
        if (_posArray.IsCreated) _posArray.Dispose();
    }

    public void SetParams(ProjectileData data, WeaponData weaponData)
    {
        _data = data;
        _weaponData = weaponData;
    }

    public void Launch(Collider parent, Transform aimTargetTransform, int accuracy)
    {
        _parent = parent;
        _parentHealth = parent.GetComponentInParent<HealthComponent>();
        _threatNotified = false;
        _transform.parent = null;
        var direction = GetDirection(aimTargetTransform);
        var maxSpreadAngle = _data.LaunchSettings.MaxSpreadAngles; 
        var spreadAngle = maxSpreadAngle * (1f - accuracy / 100f);
        var randomCircle = Random.insideUnitCircle.normalized;

        var right = Vector3.Cross(direction, Vector3.up).normalized;
        if (right == Vector3.zero)
            right = Vector3.Cross(direction, Vector3.forward).normalized;

        var up = Vector3.Cross(right, direction).normalized;

        var offsetDir = (right * randomCircle.x + up * randomCircle.y).normalized;

        var spreadRotation = Quaternion.AngleAxis(Random.Range(0f, spreadAngle), offsetDir);
        var finalDirection = (spreadRotation * direction).normalized;

        _velocity = finalDirection * _data.WeaponParams.AttackSpeed;

        Destroy(gameObject, _data.LaunchSettings.Lifetime);
    }

    private Vector3 GetDirection(Transform aimTargetTransform)
    {
        var direction = (aimTargetTransform.position - _transform.position).normalized;
        return direction;
    }

    private void FixedUpdate()
    {
        if (_stuck || _velocity.magnitude < 0.1f) return;

        RunPhysicsJob();
        UpdateRotation();
        DetectThreatsNearby(); 
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
            gravity = _data.LaunchSettings.Gravity,
            drag = _data.LaunchSettings.Drag,
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
    
    private void DetectThreatsNearby()
    {
        if (_threatNotified) return;
    
        var hitCount = Physics.OverlapSphereNonAlloc(
            _transform.position, 
            ThreatDetectionRadius,
            _threatResults, 
            _characterLayerMask
        );

        for (var i = 0; i < hitCount; i++)
        {
            var hit = _threatResults[i];
            if (hit == null) continue;

            var damageable = hit.GetComponent<IDamageable>();
            if (damageable == null) continue;
        
            if (damageable == _parentHealth) continue;

            damageable.OnDamageAttempt?.Invoke(_parent.transform);
        }
    
        _threatNotified = true; 
    }
    
    private void DetectCollisions()
    {
        var center = _transform.position + _transform.forward * _data.WeaponParams.HitBoxForwardOffset;
        var halfExtents = _data.WeaponParams.HitBoxSize * 0.5f;
        var orientation = _transform.rotation;

        var hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, _hitResults, orientation, _data.LaunchSettings.LayerMask);

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
        var damaged = hit.GetComponent<HitBodyPart>();
        var hitDirection = _velocity.normalized;

        _velocity = Vector3.zero;
        _stuck = true;
        
        var closest = hit.ClosestPoint(_transform.position);
        _transform.position = closest - hitDirection * _data.LaunchSettings.StickOffset;

        if (damaged == null)
        {
           return;
        }
        var totalDamage = _data.WeaponParams.Damage + _weaponData.WeaponParams.Damage;
        damaged.Damage(totalDamage, _data.WeaponParams.DamageType, _parent.transform);

        if (damaged.HitBodyPartSettings.Health.IsDestroyed)
        {
            Destroy(gameObject);
        }
        else if (damaged.HitBodyPartSettings.Health.IsHitReactionEnabled)
        {
            _transform.SetParent(damaged.HitBodyPartSettings.Transform);
        }
        else
        {
            Destroy(gameObject);
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