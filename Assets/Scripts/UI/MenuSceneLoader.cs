using UnityEngine;
using Zenject;

public class MenuSceneLoader : MonoBehaviour
{
   private SceneContainer _sceneContainer;

   [Inject]
   private void Construct(SceneContainer sceneContainer)
   {
      _sceneContainer = sceneContainer;
   }

   public void Load(string sceneName)
   {
      _sceneContainer.LoadScene(sceneName);
   }
}
