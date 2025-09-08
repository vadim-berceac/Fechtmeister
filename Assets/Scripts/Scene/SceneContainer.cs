using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneContainer", menuName = "Scriptable Objects/SceneContainer")]
public class SceneContainer : ScriptableObject
{
    [field: SerializeField] public SceneData[] Scenes { get; private set; }

    public void LoadScene(string sceneName)
    {
        var scene = Scenes.FirstOrDefault(s => s.SceneName == sceneName);
        SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Single);
    }
}

[System.Serializable]
public struct SceneData
{
    [field: SerializeField] public string SceneName { get; private set; }
}
