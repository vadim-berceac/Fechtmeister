using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInput : MonoBehaviour, IUIInputSet
{
   [SerializeField] private InputActionAsset inputActionAsset;
   [SerializeField] private UIActionsNames uiActionsNames;

   private readonly List<InputAction> _actions = new ();
   private InputAction _onSubmit;
   private InputAction _onCancel;
   private InputAction _onLeftMouseClick;
   private InputAction _onRightMouseClick;
   private InputAction _point;
   private InputAction _onScrollWheel;
   private InputAction _onPause;
   
   public event Action OnSubmit;
   public event Action OnCancel;
   public event Action OnLeftMouseClick;
   public event Action OnRightMouseClick;
   public event Action OnPause;
   public event Action<Vector2> OnPoint;
   public event Action<Vector2> OnScrollWheelValue;

   private void Awake()
   {
      FindActions();
      Enable();
      Subscribe();
      
#if UNITY_EDITOR
      Debug.LogWarning("UIInput: Awake");
#endif
   }

   public void FindActions()
   {
      if (inputActionAsset == null)
      {
         Debug.LogError("InputActionAsset is not assigned in the inspector.", this);
         return;
      }

      _onSubmit = inputActionAsset.FindAction(uiActionsNames.Submit);
      _onCancel = inputActionAsset.FindAction(uiActionsNames.Cancel);
      _onLeftMouseClick = inputActionAsset.FindAction(uiActionsNames.LeftMouseClick);
      _onRightMouseClick = inputActionAsset.FindAction(uiActionsNames.RightMouseClick);
      _point = inputActionAsset.FindAction(uiActionsNames.Point);
      _onScrollWheel = inputActionAsset.FindAction(uiActionsNames.ScrollWheel);
      _onPause = inputActionAsset.FindAction(uiActionsNames.Pause);

      _actions.Clear();
      _actions.AddRange(new[] { _onSubmit, _onCancel, _onLeftMouseClick, _onRightMouseClick, _point, _onScrollWheel, _onPause });
   }
   
   public void Enable()
   {
      foreach (var action in _actions)
      {
         action?.Enable();
      }
   }

   public void Disable()
   {
      foreach (var action in _actions)
      {
         action?.Disable();
      }
   }
   
   public void Subscribe()
   {
      _onSubmit.performed += OnSubmitCTX;
      _onCancel.performed += OnCancelCTX;
      _onLeftMouseClick.performed += OnLeftMouseClickCTX;
      _onRightMouseClick.performed += OnRightMouseClickCTX;
      
      _point.performed += OnPointCTX;
      _point.canceled += OnPointCTXCancel;
      
      _onScrollWheel.performed += OnOnScrollWheelCtx;
      _onScrollWheel.canceled += OnOnScrollWheelValueCancel;
      
      _onPause.performed += OnOnPauseCtx;
   }

   public void Unsubscribe()
   {
      _onSubmit.performed -= OnSubmitCTX;
      _onCancel.performed -= OnCancelCTX;
      _onLeftMouseClick.performed -= OnLeftMouseClickCTX;
      _onRightMouseClick.performed -= OnRightMouseClickCTX;
      
      _point.performed -= OnPointCTX;
      _point.canceled -= OnPointCTXCancel;
      
      _onScrollWheel.performed -= OnOnScrollWheelCtx;
      _onScrollWheel.canceled -= OnOnScrollWheelValueCancel;
      
      _onPause.performed -= OnOnPauseCtx;
   }

   private void OnSubmitCTX(InputAction.CallbackContext ctx)
   {
      OnSubmit?.Invoke();
   }

   private void OnCancelCTX(InputAction.CallbackContext ctx)
   {
      OnCancel?.Invoke();
   }

   private void OnLeftMouseClickCTX(InputAction.CallbackContext ctx)
   {
      OnLeftMouseClick?.Invoke();
   }

   private void OnRightMouseClickCTX(InputAction.CallbackContext ctx)
   {
      OnRightMouseClick?.Invoke();
   }

   private void OnPointCTX(InputAction.CallbackContext ctx)
   {
      OnPoint?.Invoke(ctx.ReadValue<Vector2>());
   }

   private void OnPointCTXCancel(InputAction.CallbackContext ctx)
   {
      OnPoint?.Invoke(Vector2.zero);
   }

   private void OnOnScrollWheelCtx(InputAction.CallbackContext ctx)
   {
      OnScrollWheelValue?.Invoke(ctx.ReadValue<Vector2>());
   }

   private void OnOnScrollWheelValueCancel(InputAction.CallbackContext ctx)
   {
      OnScrollWheelValue?.Invoke(Vector2.zero);
   }

   private void OnOnPauseCtx(InputAction.CallbackContext ctx)
   {
      OnPause?.Invoke();
   }

   private void OnDisable()
   {
      Unsubscribe();
      Disable();
   }
   
   private void OnDestroy()
   {
      OnSubmit = null;
      OnCancel = null;
      OnLeftMouseClick = null;
      OnRightMouseClick = null;
      OnPause = null;
      OnPoint = null;
      OnScrollWheelValue = null;
   }
}

[System.Serializable]
public struct UIActionsNames
{
   [field: SerializeField] public string Submit { get; private set; }
   [field: SerializeField] public string Cancel { get; private set; }
   [field: SerializeField] public string LeftMouseClick { get; private set; }
   [field: SerializeField] public string RightMouseClick { get; private set; }
   [field: SerializeField] public string Point { get; private set; }
   [field: SerializeField] public string ScrollWheel { get; private set; }
   [field: SerializeField] public string Pause { get; private set; }
}
