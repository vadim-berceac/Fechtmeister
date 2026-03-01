using UnityEngine;
using Zenject;

public class CharacterInfoComponent : MonoBehaviour
{
   private SceneCharacterContainer _sceneCharacterContainer;
   public CharacterInfo CharacterInfo { get; private set; }

   [Inject]
   private void Construct(SceneCharacterContainer sceneCharacterContainer, CharacterPresetLoader characterPresetLoader,
       HealthComponent healthComponent, CharacterCore characterCore)
   {
       _sceneCharacterContainer = sceneCharacterContainer;
       
       CharacterInfo = new(characterPresetLoader.CharacterPersonalityData.NamingSettings.CharacterName,
           characterPresetLoader.CharacterPersonalityData.Faction, characterCore, healthComponent);
   }
   
   private void OnEnable()
   {
       _sceneCharacterContainer.Add(CharacterInfo);
   }
   
   private void OnDestroy()
   {
       _sceneCharacterContainer.Remove(CharacterInfo);
   }
}