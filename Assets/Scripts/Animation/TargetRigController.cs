using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;

public class TargetRigController : ManagedUpdatableObject
{
   [field: SerializeField] public CharacterCore CharacterCore { get; set; }
   
   [field: SerializeField] public PlayableGraphCore GraphCore { get; set; }

   [field: SerializeField, Tooltip("Reference to the ItemTargeting component")]
   public ItemTargeting ItemTargeting { get; set; }

   [field: SerializeField, Tooltip("Speed of LookAt weight transition")]
   public float WeightTransitionSpeed { get; set; } = 5f;

   [field: SerializeField, Tooltip("Speed of movement towards the target (units per second)")]
   public float Speed { get; set; } = 10f;

   [field: SerializeField] public Vector3 CharacterTargetingOffset { get; set; } = new Vector3(0f, 2f, 0f);

   [Header("LookAt Bones")]
   [SerializeField] private HumanBodyBones[] lookAtBones = new HumanBodyBones[]
   {
      HumanBodyBones.Head
   };

   [SerializeField] private float[] boneWeightMultipliers = new float[]
   {
      1f
   };

   private Transform _cachedTransform;
   private Vector3 _targetPosition;
   private Vector3 _startPosition;

   private TransformAccessArray _transformAccess;
   private JobHandle _moveHandle;

   private float _currentTargetWeight = 0f;

   private void Awake()
   {
      _cachedTransform = transform;
      _startPosition = _cachedTransform.localPosition;

      _transformAccess = new TransformAccessArray(1);
      _transformAccess.Add(_cachedTransform);
   }

   private void OnDestroy()
   {
      _moveHandle.Complete();
      _transformAccess.Dispose();
   }

   public override void OnManagedUpdate()
   {
      UpdateTargetPosition();
      
      var job = new MoveToTargetJob
      {
         TargetLocalPosition = _targetPosition,
         SpeedDelta = Speed * Time.deltaTime
      };

      _moveHandle = job.Schedule(_transformAccess);
   }

   public override void OnManagedLateUpdate()
   {
      _moveHandle.Complete();

      if (GraphCore == null) return;

      UpdateLookAtWeights();
   }

   private void UpdateTargetPosition()
   {
      Transform currentTarget = null;
      var isCharacterTarget = false;

      if (ItemTargeting != null && ItemTargeting.IsAllowed)
      {
         currentTarget = ItemTargeting.GetFirstTarget();
      }

      if (currentTarget == null)
      {
         _targetPosition = _startPosition;
         return;
      }

      var targetWorldPos = isCharacterTarget 
         ? currentTarget.position + CharacterTargetingOffset 
         : currentTarget.position;

      _targetPosition = _cachedTransform.parent != null 
         ? _cachedTransform.parent.InverseTransformPoint(targetWorldPos) 
         : targetWorldPos;
   }

   private void UpdateLookAtWeights()
   {
      float targetWeight = GetDesiredWeight();

      _currentTargetWeight = Mathf.Lerp(
         _currentTargetWeight, 
         targetWeight, 
         WeightTransitionSpeed * Time.deltaTime
      );

      for (int i = 0; i < lookAtBones.Length && i < boneWeightMultipliers.Length; i++)
      {
         float boneWeight = _currentTargetWeight * boneWeightMultipliers[i];
         GraphCore.SetLookAtBoneWeight(lookAtBones[i], boneWeight);
      }
   }

   private float GetDesiredWeight()
   {
      if (CharacterCore?.CurrentState == null)
      {
         return 0f;
      }

      return CharacterCore.CurrentState.TargetingRigWeight;
   }

   public void SetLookAtEnabled(bool enabled)
   {
      _currentTargetWeight = enabled ? 1f : 0f;
   }

   public void SetTargetManually(Vector3 localPosition)
   {
      _targetPosition = localPosition;
   }
}

[BurstCompile]
public struct MoveToTargetJob : IJobParallelForTransform
{
   public Vector3 TargetLocalPosition;
   public float SpeedDelta;

   public void Execute(int index, TransformAccess transform)
   {
      var current = transform.localPosition;
      var distance = (current - TargetLocalPosition).magnitude;
      
      if (distance < 0.01f)
      {
         transform.localPosition = TargetLocalPosition;
         return;
      }

      transform.localPosition = Vector3.MoveTowards(
         current,
         TargetLocalPosition,
         SpeedDelta
      );
   }
}