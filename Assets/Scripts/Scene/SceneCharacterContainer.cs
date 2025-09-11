using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneCharacterContainer : MonoBehaviour
{
   private readonly Dictionary<Collider, CharacterCore> _chars = new ();
   private CharacterCore _currentCharacter;
   
   public CharacterCore GetNextCharacter()
   {
      if (_chars.Count == 0)
         return null;

      if (_currentCharacter == null)
      {
         _currentCharacter = _chars.First().Value;
         return _currentCharacter;
      }

      var iterator = _chars.GetEnumerator();
      var foundCurrent = false;
      CharacterCore nextCharacter = null;

      while (iterator.MoveNext())
      {
         if (foundCurrent)
         {
            nextCharacter = iterator.Current.Value;
            break;
         }
         if (iterator.Current.Value == _currentCharacter)
         {
            foundCurrent = true;
         }
      }
      _currentCharacter = nextCharacter ?? _chars.First().Value;
      return _currentCharacter;
   }

   public CharacterCore GetCharacter(Collider col)
   {
      _chars.TryGetValue(col, out var character);
      return character;
   }

   public Dictionary<Collider, CharacterCore> GetCharacters()
   {
      return _chars;
   }

   public void Add(CharacterCore character, Collider col)
   {
      _chars.Add(col, character);
   }

   public void Remove(CharacterCore character)
   {
      var entry = _chars.FirstOrDefault(x => x.Value == character);
      if (entry.Key != null)
      {
         _chars.Remove(entry.Key);
      }
   }
}
