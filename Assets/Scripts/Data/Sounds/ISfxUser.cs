
using UnityEngine;

public interface ISfxUser
{
    public string SfxSetName { get; set; }
    public SfxSet SfxSet { get; set; }

    public void PlayRandomSfx(Vector3 position);
}
