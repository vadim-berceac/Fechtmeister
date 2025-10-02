using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using UnityEngine.Animations.Rigging;

public class HeadTargetRigController : MonoBehaviour
{
   [field: SerializeField] public CharacterCore CharacterCore { get; set; }
   [field: SerializeField] public Rig ControlRig { get; set; }
   [field: SerializeField, Tooltip("Reference to the CharacterTargeting component")]
   public CharacterTargeting CharacterTargeting { get; set; }

   [field: SerializeField, Tooltip("Reference to the ItemTargeting component")]
   public ItemTargeting ItemTargeting { get; set; }

   [field: SerializeField, Tooltip("Speed of movement towards the target (units per second)")]
   public float Speed { get; set; } = 1f;

   [field: SerializeField] public Vector3 CharacterTargetingOffset { get; set; } = new Vector3(0f, 2f, 0f);

   private Transform _cachedTransform;
   private Vector3 _targetPosition;
   private Vector3 _startPosition;

   private TransformAccessArray _transformAccess;
   private JobHandle _moveHandle;

   private void Awake()
   {
      _cachedTransform = transform;
      _startPosition = _cachedTransform.localPosition;

      _transformAccess = new TransformAccessArray(1);
      _transformAccess.Add(_cachedTransform);

      if (CharacterTargeting == null || ItemTargeting == null)
      {
         Debug.LogError($"[{name}] CharacterTargeting or ItemTargeting is not assigned!");
      }
   }

   private void OnDestroy()
   {
      _moveHandle.Complete();
      _transformAccess.Dispose();
   }

   private void Update()
   {
      UpdateRigWeight();
      UpdateTargetPosition();

      var job = new MoveToTargetJob
      {
         TargetLocalPosition = _targetPosition,
         SpeedDelta = Speed * Time.deltaTime
      };

      _moveHandle = job.Schedule(_transformAccess);
   }

   private void UpdateRigWeight()
   {
      ControlRig.weight = CharacterCore.CurrentState.TargetingRigWeight;
   }

   private void LateUpdate()
   {
      _moveHandle.Complete();
   }

   private void UpdateTargetPosition()
   {
      Transform currentTarget = null;
      var isCharacterTarget = false;

      if (CharacterTargeting.IsAllowed)
      {
         currentTarget = CharacterTargeting.GetFirstTarget();
         if (currentTarget != null)
         {
            isCharacterTarget = true;
         }
      }

      if (currentTarget == null && ItemTargeting.IsAllowed)
      {
         currentTarget = ItemTargeting.GetFirstTarget();
      }

      if (currentTarget == null)
      {
         _targetPosition = _startPosition;
         return;
      }

      var targetWorldPos = isCharacterTarget ? currentTarget.position + CharacterTargetingOffset : currentTarget.position;

      _targetPosition = _cachedTransform.parent != null 
         ? _cachedTransform.parent.InverseTransformPoint(targetWorldPos) 
         : targetWorldPos;
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