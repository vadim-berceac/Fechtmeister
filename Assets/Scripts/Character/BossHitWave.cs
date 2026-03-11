using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using Zenject;

public class BossHitWave : MonoBehaviour
{
   [SerializeField] private CharacterCore bossCore;
   
   [SerializeField] private float delay;
   [SerializeField] private float damage;
   
   [SerializeField] private HealthComponent playerHealth;
   [SerializeField] private Collider playerCollider;
   
   [SerializeField] private ParticleSystem particle;
   [SerializeField] private AudioSource audioSource;
   
   
   [SerializeField] private float shakeIntensity = 1f;

   private CinemachineImpulseSource _cameraShakeSource;
   private const string AttackStateName = "FastAttackState";
   private State _attackState;
   private bool _subscribed;
   private Collider _waveCollider;
   private Coroutine _hitCoroutine;

   [Inject]
   private void Construct(CinemachineImpulseSource cameraShakeSource)
   {
      _cameraShakeSource = cameraShakeSource;
      
      _waveCollider = GetComponent<Collider>();
      
      bossCore.OnStateChanged += OnStateChanged;
   }

   private void OnStateChanged(State state)
   {
      if (state.name is not AttackStateName)
      {
         return;
      }
      if(_subscribed) return;
      
      _attackState = state;
      _attackState.ActionTime += OnActionTime;
      _subscribed = true;
   }

   private void OnActionTime()
   {
      particle?.Play();
      audioSource.Play();
      
      // Если уже идет отсчет, отменяем предыдущий
      if (_hitCoroutine != null)
      {
         StopCoroutine(_hitCoroutine);
      }
      
      _hitCoroutine = StartCoroutine(WaitAndCheckHit());
   }

   private IEnumerator WaitAndCheckHit()
   {
      yield return new WaitForSeconds(delay);
      
      // Проверяем пересечение коллайдеров
      if (_waveCollider != null && playerCollider != null && 
          Physics.ComputePenetration(_waveCollider, transform.position, transform.rotation,
                                     playerCollider, playerCollider.transform.position, 
                                     playerCollider.transform.rotation,
                                     out Vector3 direction, out float distance))
      {
         // Коллайдеры пересекаются - наносим урон
         playerHealth.Damage(damage, DamageTypes.Fire);
         
         // Проигрываем эффект тряски камеры только при попадании
         PlayCameraShake();
      }
      
      _hitCoroutine = null;
   }

   private void PlayCameraShake()
   {
      if (_cameraShakeSource != null)
      {
         _cameraShakeSource.GenerateImpulse(shakeIntensity);
      }
      else
      {
         Debug.LogWarning("BossHitWave: CinemachineImpulseSource not assigned!");
      }
   }

   private void OnDestroy()
   {
      bossCore.OnStateChanged -= OnStateChanged;
      if (_attackState != null)
      {
         _attackState.ActionTime -= OnActionTime;
      }
      
      if (_hitCoroutine != null)
      {
         StopCoroutine(_hitCoroutine);
      }
   }
}