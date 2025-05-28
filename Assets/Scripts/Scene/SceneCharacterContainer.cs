using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneCharacterContainer : MonoBehaviour
{
   private readonly HashSet<CharacterCore> _characters  = new ();
   private CharacterCore _currentCharacter;
   
   public CharacterCore GetNextCharacter()
   {
      if (_characters.Count == 0)
         return null;

      if (_currentCharacter == null)
      {
         _currentCharacter = _characters.First();
         return _currentCharacter;
      }

      var iterator = _characters.GetEnumerator();
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
      _currentCharacter = nextCharacter ?? _characters.First();
      return _currentCharacter;
   }

   public HashSet<CharacterCore> GetCharacters()
   {
      return _characters;
   }

   public void Add(CharacterCore character)
   {
      _characters.Add(character);
   }

   public void Remove(CharacterCore character)
   {
      if (!_characters.Contains(character))
      {
         return;
      }
      _characters.Remove(character);
   }
}
