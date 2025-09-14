using System.Linq;
using UnityEngine;

public class StatesContainer : MonoBehaviour
{
   [field: SerializeField] private State[] states;
   public State GetState(string stateName)
   {
      return states.FirstOrDefault(n => n.name == stateName);
   }
}