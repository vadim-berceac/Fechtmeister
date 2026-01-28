
public interface IInputSet
{
    public bool IsEnabled { get; set; }
    public void FindActions();
    public void Enable();
    public void Disable();
    public void Subscribe();
    public void Unsubscribe();
}
