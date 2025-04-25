using System;
using UnityEngine;

public interface IUIInputSet : IInputSet
{
    public event Action OnSubmit;
    public event Action OnCancel;
    public event Action OnLeftMouseClick;
    public event Action OnRightMouseClick;
    public event Action OnPause;
    public Vector2 Point { get;}
    public Vector2 ScrollWheelValue { get;}
}
