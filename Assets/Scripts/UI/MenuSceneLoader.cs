using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneLoader : MonoBehaviour
{
   public void Load(string sceneName)
   {
      SceneManager.LoadSceneAsync(sceneName);
   }
}
