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
   private InputAction _scrollWheel;
   private InputAction _pause;
   
   public event Action OnSubmit;
   public event Action OnCancel;
   public event Action OnLeftMouseClick;
   public event Action OnRightMouseClick;
   public event Action OnPause;
   public Vector2 Point { get; private set; }
   public Vector2 ScrollWheelValue { get; private set; }

   private void Awake()
   {
      FindActions();
      Enable();
      Subscribe();
      Debug.LogWarning("UIInput: Awake");
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
      _scrollWheel = inputActionAsset.FindAction(uiActionsNames.ScrollWheel);
      _pause = inputActionAsset.FindAction(uiActionsNames.Pause);

      _actions.Clear();
      _actions.AddRange(new[] { _onSubmit, _onCancel, _onLeftMouseClick, _onRightMouseClick, _point, _scrollWheel, _pause });
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
      
      _scrollWheel.performed += OnScrollWheelCTX;
      _scrollWheel.canceled += OnScrollWheelValueCancel;
      
      _pause.performed += OnPauseCTX;
   }

   public void Unsubscribe()
   {
      _onSubmit.performed -= OnSubmitCTX;
      _onCancel.performed -= OnCancelCTX;
      _onLeftMouseClick.performed -= OnLeftMouseClickCTX;
      _onRightMouseClick.performed -= OnRightMouseClickCTX;
      
      _point.performed -= OnPointCTX;
      _point.canceled -= OnPointCTXCancel;
      
      _scrollWheel.performed -= OnScrollWheelCTX;
      _scrollWheel.canceled -= OnScrollWheelValueCancel;
      
      _pause.performed -= OnPauseCTX;
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
      Point = ctx.ReadValue<Vector2>();
   }

   private void OnPointCTXCancel(InputAction.CallbackContext ctx)
   {
      Point = Vector2.zero;
   }

   private void OnScrollWheelCTX(InputAction.CallbackContext ctx)
   {
      ScrollWheelValue = ctx.ReadValue<Vector2>();
   }

   private void OnScrollWheelValueCancel(InputAction.CallbackContext ctx)
   {
      ScrollWheelValue = Vector2.zero;
   }

   private void OnPauseCTX(InputAction.CallbackContext ctx)
   {
      OnPause?.Invoke();
   }

   private void OnDisable()
   {
      Unsubscribe();
      Disable();
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
