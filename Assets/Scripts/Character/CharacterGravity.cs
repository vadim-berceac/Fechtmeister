

public class CharacterGravity
{
    private GravitySettings _gravitySettings;
    public float MaxHeightReached { get; private set; }
    public bool Grounded { get; private set; }
    public bool WasGroundedLastFrame { get; private set; }

    public void SetGrounded(bool value)
    {
        Grounded = value;
    }

    public void SetMaxHeightReached(float heightReached)
    {
        MaxHeightReached = heightReached;
    }

    public void SetWasGroundedLastFrame(bool value)
    {
        WasGroundedLastFrame = value;
    }
}
