using System.Linq;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public class StatesContainer : MonoBehaviour
{
   [field: SerializeField] private State[] states;
   
   public State GetState(string stateName)
   {
      return states.FirstOrDefault(n => n.name == stateName);
   }
   public State[] GetStates()
   {
      return states;
   }
}