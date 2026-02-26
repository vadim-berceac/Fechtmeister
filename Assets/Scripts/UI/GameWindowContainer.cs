using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameWindowContainer", menuName = "Zenject/GameWindowContainer")]
public class GameWindowContainer : ScriptableObject
{
    public HashSet<IGameWindow> GameWindows { get; private set; } = new HashSet<IGameWindow>();
}
