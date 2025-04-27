using UnityEngine;
using Zenject;

public class NextCharacterButton : MonoBehaviour
{
   private SceneCharacterContainer _sceneCharacterContainer;

   [Inject]
   private void Construct(SceneCharacterContainer sceneCharacterContainer)
   {
      _sceneCharacterContainer = sceneCharacterContainer;
   }

   public void SelectNext()
   {
      var character = _sceneCharacterContainer.GetNextCharacter();
      character.CameraTarget.SetTarget();
   }
}
