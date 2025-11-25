using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CrawlerWalkState", menuName = "States/CrawlerWalkState")]
public class CrawlerWalkState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new(character => (Mathf.Abs(character.CharacterInputHandler.InputX) == 0 &&
                              Mathf.Abs(character.CharacterInputHandler.InputY) == 0), "CrawlerIdleState"),
            new(character => (character.CharacterInputHandler.IsRun && character.Health.CurrentHealthNormalized >= 0.5), "CrawlerRunState"),
            new(c => !c.Gravity.Grounded, "CrawlerFallState"),
        };
    }
}
