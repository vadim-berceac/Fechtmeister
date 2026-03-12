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

   private void Start()
   {
      if (_sceneCharacterContainer.GetNonAICharacters().Count > 0)
      {
         SelectNext();
      }
   }

   public void SelectNext()
   {
      ResetSelection();
      var character = _sceneCharacterContainer.GetNextNonAICharacter();
      CharacterSelector.Select(character, true);
   }

   public void ResetSelection()
   {
      foreach (var character in _sceneCharacterContainer.GetNonAICharacters())
      {
         CharacterSelector.Select(character.Value, false);
      }
   }
}
