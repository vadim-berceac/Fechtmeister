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
    private AudioSource _audioSource;

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

    public void Launch(Collider parent, Transform aimTargetTransform)
    {
        _parent = parent;
        _parentHealth = parent.GetComponentInParent<HealthComponent>();
        _threatNotified = false;
        _transform.parent = null;

        if (_data.TrailPrefab != null)
            _trail = Instantiate(_data.TrailPrefab, _transform);

        var targetPos = aimTargetTransform.position;
        var displacement = targetPos - _transform.position;
        var upDir = Vector3.up;
        var horizontal = displacement - Vector3.Project(displacement, upDir);
        var d = horizontal.magnitude;
        var h = Vector3.Dot(displacement, upDir);
        var v = _data.WeaponParams.AttackSpeed;
        var g = -_data.LaunchSettings.Gravity; 

        Vector3 launchDirection;
        if (Mathf.Abs(g) < 0.01f || d < 0.01f)
        {
            launchDirection = displacement.normalized;
        }
        else
        {
            var a = (g * d * d) / (2 * v * v);
            var b = -d;
            var c = (g * d * d) / (2 * v * v) + h;
            var discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                launchDirection = displacement.normalized;
            }
            else
            {
                var sqrtD = Mathf.Sqrt(discriminant);
                var tanTheta1 = (-b + sqrtD) / (2 * a);
                var tanTheta2 = (-b - sqrtD) / (2 * a);

                var tanTheta = Mathf.Abs(tanTheta1) < Mathf.Abs(tanTheta2) ? tanTheta1 : tanTheta2;

                var theta = Mathf.Atan(tanTheta);
                var horizDir = horizontal.normalized;
                launchDirection = horizDir * Mathf.Cos(theta) + upDir * Mathf.Sin(theta);
            }
        }

        _velocity = launchDirection * v;
        _transform.rotation = Quaternion.LookRotation(launchDirection);

        Destroy(gameObject, _data.LaunchSettings.Lifetime);

        PlayFlySound();
    }

    private void PlayFlySound()
    {
        if (_data.WeaponParams.WhooshSounds== null) return;

        _audioSource.clip = _data.WeaponParams.WhooshSounds.GetRandomClip();
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
            break;
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

        PlaySound(_audioSource, _data.WeaponParams.HitSounds.GetRandomClip(), _transform);

        if (_trail) _trail.SetActive(false);

        if (damaged == null) return;

        var totalDamage = _data.WeaponParams.Damage + _weaponData.WeaponParams.Damage;
        damaged.Damage(totalDamage, _data.WeaponParams.DamageType, _parent.transform);

        if (damaged.Health.IsDestroyed)
        {
            Destroy(gameObject);
        }
        else if (damaged.Health.IsHitReactionEnabled)
        {
            _transform.SetParent(damaged.Transform);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}