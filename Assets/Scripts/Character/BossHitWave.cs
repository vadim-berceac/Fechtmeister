using UnityEngine;
using System.Collections;

public class BossHitWave : MonoBehaviour
{
   [SerializeField] private CharacterCore bossCore;
   
   [SerializeField] private float delay;
   [SerializeField] private float damage;
   
   [SerializeField] private HealthComponent playerHealth;
   [SerializeField] private Collider playerCollider;
   
   [SerializeField] private ParticleSystem particle;
   [SerializeField] private AudioSource audioSource;

   private const string AttackStateName = "FastAttackState";
   private State _attackState;
   private bool _subscribed;
   private Collider _waveCollider;
   private Coroutine _hitCoroutine;

   private void Start()
   {
      _waveCollider = GetComponent<Collider>();
      if (_waveCollider == null)
      {
         Debug.LogError("BossHitWave: Collider not found on this GameObject!");
      }
      
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
      }
      
      _hitCoroutine = null;
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