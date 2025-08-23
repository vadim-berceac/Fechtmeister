using Unity.Burst;
using UnityEngine;
using Zenject;

public class TargetingUI : MonoBehaviour
{
   [field: SerializeField] private GameObject NamePlate { get; set; }
   [field: SerializeField] private Vector2 Offset { get; set; }
   private CharacterTargetingSystem _selectedCharacterTargetingSystem;
   private Transform _itemTarget;
   private RectTransform _namePlateRectTransform;
   private SceneCamera _sceneCamera;
   private bool _isSubscribed;
   private Vector3 _worldPosition;
   private Vector3 _screenPosition;

   [Inject]
   private void Construct(SceneCamera sceneCamera)
   {
      _sceneCamera = sceneCamera;
   }

   private void Awake()
   {
      NamePlate.SetActive(false);
      _namePlateRectTransform = NamePlate.GetComponent<RectTransform>();
      CharacterSelector.OnCharacterSelected += SelectCharacter;
   }

   private void LateUpdate()
   {
      UpdateNamePlatePosition();
   }

   private void SelectCharacter(CharacterCore character)
   {
      _selectedCharacterTargetingSystem = character.TargetingSystem;
      HideNamePlate( _itemTarget);
      Unsubscribe();
      
      if (_selectedCharacterTargetingSystem == null)
      {
        return;
      }
      Debug.LogWarning("Selected character: " + character.name);
      Subscribe();
   }

   private void Subscribe()
   {
      _selectedCharacterTargetingSystem.ItemTargeting.OnTargetAdded += ShowNamePlate;
      _selectedCharacterTargetingSystem.ItemTargeting.OnTargetRemoved += HideNamePlate;
      _isSubscribed = true;
   }

   private void Unsubscribe()
   {
      _selectedCharacterTargetingSystem.ItemTargeting.OnTargetAdded -= ShowNamePlate;
      _selectedCharacterTargetingSystem.ItemTargeting.OnTargetRemoved -= HideNamePlate;
      _isSubscribed = false;
   }

   private void ShowNamePlate(Transform targetTransform)
   {
      Debug.Log(targetTransform.name);
      _itemTarget = targetTransform;
      NamePlate.SetActive(true);
   }

   private void HideNamePlate(Transform targetTransform)
   {
      if (targetTransform != _itemTarget)
      {
         return;
      }
      _itemTarget = null;
      NamePlate.SetActive(false);
   }
   
   [BurstCompile]
   private void UpdateNamePlatePosition() 
   {
      if (!NamePlate.activeSelf || _itemTarget == null || _sceneCamera == null || _sceneCamera.SceneCameraData.MainCameraCam == null)
      { 
         NamePlate.SetActive(false);
         return;
      }
      Vector3 worldPosition = _itemTarget.position + Vector3.up * 1f; 
      Vector3 screenPosition = _sceneCamera.SceneCameraData.MainCameraCam.WorldToScreenPoint(worldPosition);
      
      if (screenPosition.z < 0)
      {
         NamePlate.SetActive(false);
         return;
      }

      RectTransformUtility.ScreenPointToLocalPointInRectangle(
         _namePlateRectTransform.parent as RectTransform,
         screenPosition,
         _sceneCamera.SceneCameraData.MainCameraCam,
         out Vector2 canvasPosition
      );
      _namePlateRectTransform.anchoredPosition = canvasPosition + Offset;
   }
   
   private void OnDisable()
   {
      CharacterSelector.OnCharacterSelected -= SelectCharacter;
      if (!_isSubscribed)
      {
         return;
      }
      _selectedCharacterTargetingSystem.ItemTargeting.OnTargetAdded -= ShowNamePlate;
      _selectedCharacterTargetingSystem.ItemTargeting.OnTargetRemoved -= HideNamePlate;
   }
}
