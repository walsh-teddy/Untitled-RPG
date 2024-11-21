using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : Move
{
    public Teleport(TeleportData data) : base (data)
    {
        // extract data
        range = data.range;
        ignoreLineOfSight = data.ignoreLineOfSight;
        targetType = targetTypes.move;
        
    }

    public override void UpdatePossibleTargets()
    {
        SetUpVariables();

        possibleSpaces.Clear();
        possibleTargets.Clear();

        // Get a list of every tile within range of the teleport
        possibleSpaces = source.LevelSpawnerRef.TilesInRange(origin, range + 0.5f, source.Owner.EyeHeight, 0, true);
        // Only add targets in line of sight if thats required
        if (!ignoreLineOfSight) // This teleport needs line of sight
        {
            possibleSpaces = source.LevelSpawnerRef.LineOfSight(possibleSpaces, source.Owner, true);
        }

        foreach (Tile tile in possibleSpaces)
        {
            if (!tile.HasObstacle)
            {
                possibleTargets.Add(tile);
            }
        }

        // Actions with targetType == move, automatically allow you to click on possibleSpaces.
        //Teleports are not able to, so possibleSpaces should end empty (it was just being used as a way to hold every possible
        //tile within range and then conditionally add them to possibleTargets, which is the one that really matters)
        possibleSpaces.Clear();
    }

    public override void SetTarget(Tile target)
    {
        // Do nothing if targets are locked
        if (targetsLocked) // Targets are locked
        {
            // Do nothing
            return;
        }

        // Make sure the target is one of the valid targets already calculated
        if (!possibleTargets.Contains(target))
        {
            Debug.Log("Invalid target.");
            return;
        }

        // Record the target (both the tile and the creature)
        targets.Clear();
        targets.Add(source.Owner.Space);
        targets.Add(target);
    }

    public override string FormatDescription()
    {
        string text = "";


        if (range != 1) // Plural tiles
        {
            text += "Teleport " + range + " tiles. ";
        }
        else // Singular tile
        {
            text += "Teleport 1 tile. ";
        }

        // Ignores Line of Sight
        if (ignoreLineOfSight)
        {
            text += "Ignores line of sight. ";
        }

        return text;
    }
}
