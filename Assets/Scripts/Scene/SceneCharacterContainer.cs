using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneCharacterContainer", menuName = "Zenject/SceneCharacterContainer")]
public class SceneCharacterContainer : ScriptableObject
{
   private readonly Dictionary<Collider, CharacterInfo> _chars = new ();
   private CharacterInfo _currentCharacter;
   
   public CharacterInfo GetNextCharacter()
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
      CharacterInfo nextCharacter = null;

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
   
   public CharacterInfo GetNextNonAICharacter()
   {
      if (_chars.Count == 0)
      {
         return null;
      }

      if (_currentCharacter == null)
      {
         _currentCharacter = _chars.Values.FirstOrDefault(c => !c.Core.IsAI);
         return _currentCharacter;
      }

      var iterator = _chars.GetEnumerator();
      var foundCurrent = false;
      CharacterInfo nextCharacter = null;

      while (iterator.MoveNext())
      {
         if (foundCurrent)
         {
            if (!iterator.Current.Value.Core.IsAI)
            {
               nextCharacter = iterator.Current.Value;
               break;
            }
         }
         if (iterator.Current.Value == _currentCharacter)
         {
            foundCurrent = true;
         }
      }
      
      if (nextCharacter == null)
      {
         nextCharacter = _chars.Values.FirstOrDefault(c => !c.Core.IsAI);
      }

      _currentCharacter = nextCharacter;
      return _currentCharacter;
   }


   public CharacterInfo GetCharacter(Collider col)
   {
      _chars.TryGetValue(col, out var character);
      return character;
   }

   public Dictionary<Collider, CharacterInfo> GetCharacters()
   {
      return _chars;
   }
   
   public Dictionary<Collider, CharacterInfo> GetNonAICharacters()
   {
      var result = _chars.Where(c 
         => !c.Value.Core.IsAI).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      return result;
   }

   public void Add(CharacterInfo character)
   {
      _chars.Add(character.Core.LocomotionSettings.CharacterCollider, character);
   }

   public void Remove(CharacterInfo character)
   {
      var entry = _chars.FirstOrDefault(x => x.Value == character);
      if (entry.Key != null)
      {
         _chars.Remove(entry.Key);
      }
   }
}
