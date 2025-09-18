using UnityEngine;

public class CurrentSpeed
{
   private Vector3 _lastPosition;
   private Vector3 _currentPosition;
   private Vector3 _deltaPosition;
   private Vector3 _currentVelocity;
   private float _deltaTime;
   private readonly Transform _transform;
    
   public float CurrentHorizontalSpeed { get; private set; }
   public float CurrentVerticalSpeed { get; private set; }
   public float LastNotNullHorizontalSpeed { get; private set; }
   public float LastNotNullVerticalSpeed { get; private set; }

   public CurrentSpeed(Transform transform)
   {
      _transform = transform;
      _lastPosition = _transform.position;
      CurrentHorizontalSpeed = 0f;
      CurrentVerticalSpeed = 0f;
      LastNotNullHorizontalSpeed = 0f;
      LastNotNullVerticalSpeed = 0f;
   }

   public void OnUpdate()
   {
      CalculateCurrentSpeed();
   }

   private void CalculateCurrentSpeed()
   {
      _currentPosition = _transform.position;
      _deltaPosition = _currentPosition - _lastPosition;
      _deltaTime = Time.deltaTime;
      _currentVelocity = _deltaPosition / _deltaTime;
      CurrentHorizontalSpeed = new Vector3(_currentVelocity.x, 0, _currentVelocity.z).magnitude;
      CurrentVerticalSpeed = _currentVelocity.y;
      _lastPosition = _currentPosition;
   }

   public void StopUpdateLastHorizontalSpeed()
   {
      LastNotNullHorizontalSpeed = CurrentHorizontalSpeed;
   }
}
