using System.Collections.Generic;
using UnityEngine;

public class GameWindowContainer : MonoBehaviour
{
    public HashSet<IGameWindow> GameWindows { get; set; }

    private void Awake()
    {
        GameWindows = new HashSet<IGameWindow>();
    }
}
