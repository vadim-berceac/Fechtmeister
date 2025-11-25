using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CrawlerIdleState", menuName = "States/CrawlerIdleState")]
public class CrawlerIdleState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
             new (c => Mathf.Abs(c.CharacterInputHandler.InputX) > 0 ||
                      Mathf.Abs(c.CharacterInputHandler.InputY) > 0, "CrawlerWalkState"),
             new(c => !c.Gravity.Grounded, "CrawlerFallState"),
        };
    }
}
