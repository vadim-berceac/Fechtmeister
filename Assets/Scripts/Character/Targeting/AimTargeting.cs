using Unity.Behavior;
using UnityEngine;

public class AimTargeting : MonoBehaviour
{
   [SerializeField] private BehaviorGraphAgent agent;
   [SerializeField] private PlayableGraphCore graphCore;

   private BlackboardVariable<HealthComponent> _targetHealth;
   private Transform _currentTargetTransform;
   
   private void Start()
   {
      var blackboard = agent.BlackboardReference;
      
      if (blackboard.GetVariable("CurrentTarget", out _targetHealth))
      {
         // Подписываемся БЕЗ параметров
         _targetHealth.OnValueChanged += OnTargetChanged;
         
         // Обрабатываем начальное значение
         if (_targetHealth.Value != null)
         {
            SetTarget(_targetHealth.Value);
         }
      }
      else
      {
         Debug.LogWarning("Variable 'CurrentTarget' not found on Blackboard!");
      }
   }
    
   private void OnDestroy()
   {
      if (_targetHealth != null)
      {
         _targetHealth.OnValueChanged -= OnTargetChanged;
      }
   }

   // ИСПРАВЛЕНИЕ: метод БЕЗ параметров
   private void OnTargetChanged()
   {
      // Читаем текущее значение из переменной
      HealthComponent newTarget = _targetHealth.Value;
      
      Debug.Log($"Target changed to: {newTarget?.name ?? "null"}");
      
      if (newTarget != null)
      {
         SetTarget(newTarget);
      }
      else
      {
         ClearTarget();
      }
   }
   
   private void SetTarget(HealthComponent target)
   {
      _currentTargetTransform = target.transform;
      
      if (graphCore != null)
      {
         graphCore.SetLookAtBoneWeight(HumanBodyBones.Head, 1f);
         graphCore.SetLookAtBoneWeight(HumanBodyBones.Neck, 0.5f);
         graphCore.SetLookAtBoneWeight(HumanBodyBones.Spine, 0.3f);
      }
      
      Debug.Log($"Aiming at: {target.name}");
   }
   
   private void ClearTarget()
   {
      _currentTargetTransform = null;
      
      if (graphCore != null)
      {
         graphCore.SetLookAtBoneWeight(HumanBodyBones.Head, 0f);
         graphCore.SetLookAtBoneWeight(HumanBodyBones.Neck, 0f);
         graphCore.SetLookAtBoneWeight(HumanBodyBones.Spine, 0f);
      }
      
      Debug.Log("Target cleared");
   }
   
   public Transform GetCurrentTarget()
   {
      return _currentTargetTransform;
   }
}