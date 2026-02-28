using UnityEngine;

public class Sight : MonoBehaviour
{
   [SerializeField] private GameObject sight;
   
   public void Enable()
   {
      sight.SetActive(true);
   }

   public void Disable()
   {
      sight.SetActive(false);
   }
}
