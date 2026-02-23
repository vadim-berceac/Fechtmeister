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
    private bool _threatNotified;
    private AudioSource _audioSource; // ✅

    private const float ThreatDetectionRadius = 10f;
    private LayerMask _characterLayerMask;

    private GameObject _trail;

    private void Start()
    {
        _transform = transform;
        _audioSource = gameObject.AddComponent<AudioSource>();

        if (_characterLayerMask == 0)
            _characterLayerMask = LayerMask.GetMask("Character");
        
        PlaySound(_audioSource, _data.SpawnSound, _transform);
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

        if (_data.TrailPrefab != null)
        {
            _trail = Instantiate(_data.TrailPrefab, _transform);
        }

        var direction = (aimTargetTransform.position - _transform.position).normalized;
        var spreadAngle = _data.LaunchSettings.MaxSpreadAngles * (1f - accuracy / 100f);
        var randomCircle = Random.insideUnitCircle.normalized;

        var right = Vector3.Cross(direction, Vector3.up).normalized;
        if (right == Vector3.zero)
            right = Vector3.Cross(direction, Vector3.forward).normalized;

        var up = Vector3.Cross(right, direction).normalized;
        var offsetDir = (right * randomCircle.x + up * randomCircle.y).normalized;
        var spreadRotation = Quaternion.AngleAxis(Random.Range(0f, spreadAngle), offsetDir);
        var finalDirection = (spreadRotation * direction).normalized;

        _velocity = finalDirection * _data.WeaponParams.AttackSpeed;
        _transform.rotation = Quaternion.LookRotation(finalDirection);

        Destroy(gameObject, _data.LaunchSettings.Lifetime);

        PlayFlySound();
    }
    
    private void PlayFlySound()
    {
        if (_data.FlySound == null) return;

        _audioSource.clip = _data.FlySound;
        _audioSource.loop = true;
        _audioSource.volume = 0.2f;
        _audioSource.spatialBlend = 1f;
        _audioSource.rolloffMode = AudioRolloffMode.Custom;
        _audioSource.minDistance = 1f;   
        _audioSource.maxDistance = 30f;  
        _audioSource.SetCustomCurve(
            AudioSourceCurveType.CustomRolloff,
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0f) 
        );
        _audioSource.Play();
    }
    
    // private void PlayImpactSound()
    // {
    //     if (_audioSource != null)
    //     {
    //         _audioSource.Stop();
    //         _audioSource.loop = false;
    //     }
    //
    //     if (_data.ImpactSound == null) return;
    //
    //     AudioSource.PlayClipAtPoint(_data.ImpactSound, _transform.position);
    // }
    
    private static void PlaySound(AudioSource audioSource, AudioClip clip, Transform pos)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, pos.position);
    }

    private void FixedUpdate()
    {
        if (_stuck || _velocity.sqrMagnitude < 0.01f) return;

        RunPhysics();
        UpdateRotation();
        DetectThreatsNearby();
        DetectCollisions();
    }

    private void RunPhysics()
    {
        var dt = Time.fixedDeltaTime;
        _velocity += _data.LaunchSettings.Gravity * dt * Vector3.up;
        _velocity *= Mathf.Exp(-_data.LaunchSettings.Drag * dt);
        _transform.position += _velocity * dt;
    }

    private void UpdateRotation()
    {
        if (_velocity.sqrMagnitude > 0.01f)
            _transform.rotation = Quaternion.LookRotation(_velocity);
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

            var hitHealth = hit.GetComponentInParent<HealthComponent>();
            if (hitHealth == _parentHealth) continue;

            damageable.OnDamageAttempt?.Invoke(_parent.transform);
        }

        _threatNotified = true;
    }

    private void DetectCollisions()
    {
        var center = _transform.position + _transform.forward * _data.WeaponParams.HitBoxForwardOffset;
        var halfExtents = _data.WeaponParams.HitBoxSize * 0.5f;

        var hitCount = Physics.OverlapBoxNonAlloc(
            center, halfExtents, _hitResults,
            _transform.rotation, _data.LaunchSettings.LayerMask
        );

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
        
        PlaySound(_audioSource, _data.ImpactSound, _transform);
        
        if (_trail) _trail.SetActive(false);

        if (damaged == null) return;

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
}