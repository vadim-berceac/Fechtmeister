using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private ProjectileData _data;
    private WeaponData _weaponData;
    private Vector3 _velocity;
    private Collider _parent;
    private HealthComponent _parentHealth;
    private Transform _transform;
    private readonly Collider[] _hitResults    = new Collider[5];
    private readonly Collider[] _threatResults = new Collider[10];
    private bool _stuck;
    private bool _threatNotified;
    private AudioSource _audioSource;

    private const float ThreatDetectionRadius = 10f;
    private const float SphereCastRadius = 0.15f;
    private const int SubstepCount = 3;
    
    private LayerMask _characterLayerMask;

    private GameObject _trail;
    
    private HealthComponent _homingTarget;
    private float _homingTimer;   
    private float _acquireTimer;   
    private bool  _homingActive;
   
    private void Start()
    {
        _transform  = transform;
        
        if(_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        if (_characterLayerMask == 0)
            _characterLayerMask = LayerMask.GetMask("Character");

        PlaySound(_audioSource, _data.SpawnSound, _transform);
    }

    public void SetParams(ProjectileData data, WeaponData weaponData)
    {
        _data       = data;
        _weaponData = weaponData;
    }

    public void Launch(Collider parent, Transform aimTargetTransform)
    {
        if (_transform == null)
            _transform = transform;
        
        if(_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        if (_data.WeaponParams.StartSounds)
        {
            PlaySound(_audioSource, _data.WeaponParams.StartSounds.GetRandomClip(), _transform);
        }
        
        if (_data.WeaponParams.StartParticlePrefab)
        {
            PlayVFX(_data.WeaponParams.StartParticlePrefab);
        }
        
        _parent       = parent;
        _parentHealth = parent.GetComponentInParent<HealthComponent>();
        _threatNotified = false;
        _transform.parent = null;

        if (_data.TrailPrefab != null)
            _trail = Instantiate(_data.TrailPrefab, _transform);
      
        var h = _data.HomingSettings;
        _homingActive = h.Enabled && h.Duration > 0f;
        _homingTimer  = 0f;
        _acquireTimer = 0f;
        _homingTarget = null;
       
        var targetPos    = aimTargetTransform.position;
        var displacement = targetPos - _transform.position;
        var upDir        = Vector3.up;
        var horizontal   = displacement - Vector3.Project(displacement, upDir);
        var d = horizontal.magnitude;
        var hDist = Vector3.Dot(displacement, upDir);
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
            var c = (g * d * d) / (2 * v * v) + hDist;
            var discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                launchDirection = displacement.normalized;
            }
            else
            {
                var sqrtD    = Mathf.Sqrt(discriminant);
                var tanTheta1 = (-b + sqrtD) / (2 * a);
                var tanTheta2 = (-b - sqrtD) / (2 * a);
                var tanTheta  = Mathf.Abs(tanTheta1) < Mathf.Abs(tanTheta2) ? tanTheta1 : tanTheta2;
                var theta     = Mathf.Atan(tanTheta);
                var horizDir  = horizontal.normalized;
                launchDirection = horizDir * Mathf.Cos(theta) + upDir * Mathf.Sin(theta);
            }
        }

        _velocity          = launchDirection * v;
        _transform.rotation = Quaternion.LookRotation(launchDirection);

        Destroy(gameObject, _data.LaunchSettings.Lifetime);
        PlayFlySound();
    }

    private HealthComponent FindNearestHomingTarget()
    {
        var settings  = _data.HomingSettings;
        var hitCount  = Physics.OverlapSphereNonAlloc(
            _transform.position,
            settings.SearchRadius,
            _threatResults,
            _characterLayerMask
        );

        HealthComponent best     = null;
        float           bestDist = float.MaxValue;

        for (var i = 0; i < hitCount; i++)
        {
            var col = _threatResults[i];
            if (col == null || col == _parent) continue;

            var health = col.GetComponentInParent<HealthComponent>();
            if (health == null || health == _parentHealth) continue;
            if (health.IsDestroyed) continue;

            var damageable = col.GetComponent<IDamageable>();
            if (damageable == null) continue;

            var dist = (_transform.position - col.transform.position).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best     = health;
            }
        }

        return best;
    }

    private void UpdateHoming(float dt)
    {
        var settings = _data.HomingSettings;

        if (_acquireTimer < settings.AcquireDelay)
        {
            _acquireTimer += dt;
            return;
        }

        if (_homingTimer >= settings.Duration)
        {
            _homingActive = false;
            return;
        }

        _homingTimer += dt;

        if (_homingTarget == null || _homingTarget.IsDestroyed)
            _homingTarget = FindNearestHomingTarget();

        if (_homingTarget == null) return;

        var aimPoint = _homingTarget.DamagedObject != null
            ? _homingTarget.DamagedObject.position
            : _homingTarget.transform.position;
        
        var speed = _velocity.magnitude;
        if (speed > 0.01f)
        {
            var dist         = (aimPoint - _transform.position).magnitude;
            var timeToTarget = dist / speed;
            var gravityDrop  = 0.5f * Mathf.Abs(_data.LaunchSettings.Gravity) * timeToTarget * timeToTarget;
            aimPoint        += Vector3.up * gravityDrop;
        }

        var toTarget   = (aimPoint - _transform.position).normalized;
        var currentDir = _velocity.normalized;
        var maxAngle   = settings.TurnSpeed * dt;

        var newDir = Vector3.RotateTowards(currentDir, toTarget, maxAngle, 0f);
        _velocity  = newDir * speed;
    }

    private void FixedUpdate()
    {
        if (_stuck || _velocity.sqrMagnitude < 0.01f) return;

        var dt = Time.fixedDeltaTime / SubstepCount;
        var gravity = _data.LaunchSettings.Gravity;
        var drag = _data.LaunchSettings.Drag;
        var layerMask = _data.LaunchSettings.LayerMask;
        var gravityVector = gravity * Vector3.up;
        var dragExp = Mathf.Exp(-drag * dt);
    
        for (int i = 0; i < SubstepCount; i++)
        {
            if (DetectCollisions(layerMask)) return;
        
            _velocity += gravityVector * dt;
            _velocity *= dragExp;
            _transform.position += _velocity * dt;
            
            if (_homingActive) UpdateHoming(dt);
        }
        
        UpdateRotation();
        DetectThreatsNearby();
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
            _transform.position, ThreatDetectionRadius,
            _threatResults, _characterLayerMask
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

    private bool DetectCollisions(LayerMask layerMask)
    {
        var speed = _velocity.magnitude;
        if (speed < 0.01f) return false;
        
        if (Physics.SphereCast(_transform.position, SphereCastRadius, _velocity * (1f / speed), 
                out RaycastHit hit, speed * Time.fixedDeltaTime / SubstepCount, layerMask))
        {
            if (!hit.collider.isTrigger && hit.collider != _parent)
            {
                HandleHit(hit.collider);
                return true;
            }
        }
        return false;
    }

    private void HandleHit(Collider hit)
    {
        var damaged = hit.GetComponent<HitBodyPart>();

        Debug.Log($"[Projectile] Hit: {hit.name} | HitBodyPart: {damaged != null} | Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");
        var hitDirection = _velocity.magnitude > 0.01f ? _velocity.normalized : _transform.forward;

        _velocity = Vector3.zero;
        _stuck = true;

        var closest = hit.ClosestPoint(_transform.position);
        _transform.position = closest - hitDirection * _data.LaunchSettings.StickOffset;

        PlaySound(_audioSource, _data.WeaponParams.HitSounds.GetRandomClip(), _transform);
    
        if (_data.WeaponParams.HitParticlePrefab)
        {
            PlayVFX(_data.WeaponParams.HitParticlePrefab);
        }

        if (_trail) _trail.SetActive(false);

        // Если нет HitBodyPart (например, столкнулся со стеной/окружением) - просто уничтожаем
        if (damaged == null)
        {
            Destroy(gameObject);
            return;
        }

        // Если есть HitBodyPart - наносим урон и смотрим на результат
        var totalDamage = _data.WeaponParams.Damage + _weaponData.WeaponParams.Damage;
        damaged.Damage(totalDamage, _data.WeaponParams.DamageType, _parent.transform);

        if (damaged.Health.IsDestroyed || _data.LaunchSettings.DestroyOnHit)
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

    private void PlayVFX(GameObject vfxPrefab)
    {
        var fx = Instantiate(
            vfxPrefab,
            _transform.position,
            _transform.rotation
        );

        var ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
            Destroy(fx, ps.main.duration + ps.main.startLifetime.constantMax);
        else
            Destroy(fx, 5f);
    }

    private void PlayFlySound()
    {
        if (_data.WeaponParams.WhooshSounds == null) return;

        _audioSource.clip         = _data.WeaponParams.WhooshSounds.GetRandomClip();
        _audioSource.loop         = true;
        _audioSource.volume       = 0.2f;
        _audioSource.spatialBlend = 1f;
        _audioSource.rolloffMode  = AudioRolloffMode.Custom;
        _audioSource.minDistance  = 1f;
        _audioSource.maxDistance  = 30f;
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
}