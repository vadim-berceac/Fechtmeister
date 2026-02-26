using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "StatesContainer", menuName = "Zenject/StatesContainer")]
public class StatesContainer : ScriptableObject
{
   [field: SerializeField] private AvatarMasksSettings avatarMasksSettings;
   [field: SerializeField] private StatesSet[] statesSets;

   public StatesSet GetStateSet(StateMachineType stateMachineType)
   {
      return statesSets.FirstOrDefault(s => s.StateMachineType == stateMachineType);
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

[System.Serializable]
public struct StatesSet
{
   [field: SerializeField] public StateMachineType StateMachineType { get; private set; }
   [field: SerializeField] public int StartStateIndex { get; private set; }
   [field: SerializeField] public int StartSubStateIndex { get; private set; }   
   [field: SerializeField] private State[] States { get; set; }
   
   public State GetState(string stateName)
   {
      return States.FirstOrDefault(n => n.name == stateName);
   }

   public State GetStartState()
   {
      return States[StartStateIndex];
   }

   public State GetStartSubState()
   {
      return States[StartSubStateIndex];
   }
}

public enum StateMachineType
{
   Humanoid = 0,
   Crawler = 1
}