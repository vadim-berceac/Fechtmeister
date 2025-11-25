using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CrawlerRunState", menuName = "States/CrawlerRunState")]
public class CrawlerRunState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => !character.CharacterInputHandler.IsRun, "CrawlerWalkState"),
            new(c => !c.Gravity.Grounded, "CrawlerFallState"),
        };
    }
}
