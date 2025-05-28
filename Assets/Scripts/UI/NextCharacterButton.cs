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
      ResetSelection();
      var character = _sceneCharacterContainer.GetNextCharacter();
      character.Select(true);
   }

   public void ResetSelection()
   {
      foreach (var character in _sceneCharacterContainer.GetCharacters())
      {
         character.Select(false);
      }
   }
}
