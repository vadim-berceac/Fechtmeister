using UnityEngine;

public class CurrentSpeed
{
   private readonly CharacterController _controller;
   private readonly Animator _animator;
   
   public float CurrentHorizontalSpeed { get; private set; }
   public float CurrentVerticalSpeed { get; private set; }
   
   public float LastNotNullHorizontalSpeed { get; private set; }
   public float LastNotNullVerticalSpeed { get; private set; }

   public CurrentSpeed(CharacterController controller, Animator animator)
   {
      _controller = controller;
      _animator = animator;
      CurrentHorizontalSpeed = 0f;
      CurrentVerticalSpeed = 0f;
      LastNotNullHorizontalSpeed = 0f;
      LastNotNullVerticalSpeed = 0f;
   }

   public void OnUpdate()
   {
      CurrentHorizontalSpeed = new Vector3 (_controller.velocity.x, 0, _controller.velocity.z).magnitude;
      CurrentVerticalSpeed = _controller.velocity.y;
      OnAnimatorUpdate();
   }

   public void StopUpdateLastHorizontalSpeed()
   {
      LastNotNullHorizontalSpeed = CurrentHorizontalSpeed;
      Debug.Log(LastNotNullHorizontalSpeed);
   }
   
   private void OnAnimatorUpdate()
   {
      _animator.SetFloat(AnimationParams.Speed, CurrentHorizontalSpeed);
   }
}
