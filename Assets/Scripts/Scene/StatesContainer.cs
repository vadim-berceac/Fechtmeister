using System.Linq;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public class StatesContainer : MonoBehaviour
{
   [field: SerializeField] private AvatarMasksSettings avatarMasksSettings;
   [field: SerializeField] private State[] states;
   
   public State GetState(string stateName)
   {
      return states.FirstOrDefault(n => n.name == stateName);
   }
   public State[] GetStates()
   {
      return states;
   }

   public AvatarMasksSettings GetAvatarMasksSettings()
   {
      return avatarMasksSettings;
   }
}

[System.Serializable]
public struct AvatarMasksSettings
{
   [field: SerializeField] public AvatarMask FullBodyMask { get; private set; }
   [field: SerializeField] public AvatarMask UpperBodyMask { get; private set; }
}