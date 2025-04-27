using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneCharacterContainer : MonoBehaviour
{
   public HashSet<CharacterCore> Characters { get; private set; } = new HashSet<CharacterCore>();
   private CharacterCore _currentCharacter = null;
   
   public CharacterCore GetNextCharacter()
   {
      if (Characters.Count == 0)
         return null;

      if (_currentCharacter == null)
      {
         _currentCharacter = Characters.First();
         return _currentCharacter;
      }

      var iterator = Characters.GetEnumerator();
      var foundCurrent = false;
      CharacterCore nextCharacter = null;

      while (iterator.MoveNext())
      {
         if (foundCurrent)
         {
            nextCharacter = iterator.Current;
            break;
         }
         if (iterator.Current == _currentCharacter)
         {
            foundCurrent = true;
         }
      }
      _currentCharacter = nextCharacter ?? Characters.First();
      return _currentCharacter;
   }
}
