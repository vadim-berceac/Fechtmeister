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
      CharacterSelector.Select(character, true);
   }

   public void ResetSelection()
   {
      foreach (var character in _sceneCharacterContainer.GetCharacters())
      {
         CharacterSelector.Select(character.Value, false);
      }
   }
}
