using System;
using UnityEngine;

public interface IUIInputSet : IInputSet
{
    public event Action OnSubmit;
    public event Action OnCancel;
    public event Action OnLeftMouseClick;
    public event Action OnRightMouseClick;
    public event Action OnPause;
    public event Action<Vector2> OnPoint;
    public event Action<Vector2> OnScrollWheelValue;
}
