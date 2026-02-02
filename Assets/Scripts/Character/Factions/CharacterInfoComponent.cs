using UnityEngine;
using Zenject;

public class CharacterInfoComponent : MonoBehaviour
{
   [SerializeField] private CharacterInfoSettings characterInfoSettings;
   private SceneCharacterContainer _sceneCharacterContainer;
   public CharacterInfo CharacterInfo { get; set; }

   [Inject]
   private void Construct(SceneCharacterContainer sceneCharacterContainer)
   {
       _sceneCharacterContainer = sceneCharacterContainer;
       
       CharacterInfo = new(characterInfoSettings.CharacterPresetLoader.CharacterPersonalityData.NamingSettings.CharacterName,
           characterInfoSettings.CharacterPresetLoader.CharacterPersonalityData.Faction, characterInfoSettings.Core,
           characterInfoSettings.Health);
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

[System.Serializable]
public struct CharacterInfoSettings
{
   [field: SerializeField] public CharacterPresetLoader CharacterPresetLoader { get; set; }
   [field: SerializeField] public HealthComponent Health { get; set; }
   [field: SerializeField] public CharacterCore Core { get; set; }
}
