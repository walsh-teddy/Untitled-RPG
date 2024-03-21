using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Creature
{
    // TODO: Add variables for decision making

    public override void AI()
    {
        // TODO: uhhhhhhhhhhhhhhhhhhhhhhhhhh

        // Get a list of every player in range
        List<Tile> validTargets = activeActionSources[0].ActionList[1].PossibleTargets;

        // If there are no valid targets, path to the nearest player
        if (validTargets.Count == 0)
        {
            // Get a list of all players
            List<Creature> players = AllEnemies();

            // Find the nearest one and move to it
            SubmitAction(Pathfind(players[0].Space));
        }
        else // There are enemies in attack range
        {
            activeActionSources[0].ActionList[1].SetTarget(validTargets[0]);
            SubmitAction(activeActionSources[0].ActionList[1]);
        }
    }

    public virtual Action Pathfind(Tile targetTile)
    {
        // TODO: Actually make this lol
        // Probably use Daikstra
        return activeActionSources[0].ActionList[0];
    }
}
