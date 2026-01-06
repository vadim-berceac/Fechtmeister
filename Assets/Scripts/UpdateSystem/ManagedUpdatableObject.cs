using UnityEngine;

public interface IUpdatable
{
    Transform Transform { get; }
    void OnManagedUpdate();
    void OnManagedFixedUpdate();
    void OnManagedLateUpdate();
}

public enum UpdateLOD
{
    Full,           // Каждый кадр (1/1)
    Medium,         // Каждый 2-3 кадр (2/3)
    Low,            // Каждый 3 кадр (1/3)
    Paused          // Не обновляется
}

public abstract class ManagedUpdatableObject : MonoBehaviour, IUpdatable
{
    public Transform Transform => transform;

    protected virtual void OnEnable()
    {
        if (CentralizedUpdateSystem.Instance != null)
            CentralizedUpdateSystem.Instance.Register(this);
    }

    protected virtual void OnDisable()
    {
        if (CentralizedUpdateSystem.Instance != null)
            CentralizedUpdateSystem.Instance.Unregister(this);
    }

    public abstract void OnManagedUpdate();
    public virtual void OnManagedFixedUpdate() { }
    public virtual void OnManagedLateUpdate() { }
}
