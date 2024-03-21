using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttack : Attack
{

    public AOEAttack(string displayName, int cooldownCost, int rechargeCost, int energyCost, int castTimeCost,
        int hitBonusBase, List<Game.stats> hitBonusScale, int critBonusBase, List<Game.stats> critBonusScale,
        int damage, aoeTypes aoeType, float range, float aoeReach, float aoeAngle, float aoeHeight, List<attackEffects> extraEffects)
        : base(displayName, cooldownCost, rechargeCost, energyCost, castTimeCost, hitBonusBase, hitBonusScale, 
            critBonusBase, critBonusScale, damage, range, extraEffects)
    {
        // AOE Variables
        this.aoeType = aoeType;
        this.aoeReach = aoeReach;
        this.aoeAngle = aoeAngle;
        this.aoeHeight = aoeHeight;
        isAOE = true;

        if (aoeType == aoeTypes.circle)
        {
            originatesFromAttacker = false;
        }
    }

    // Mark the center tile instead of the actual targets of the attacks
    public override void SetTarget(Tile target)
    {
        // Set the target tile
        aoeTargetTile = target;

        // If its a circle AOE, make sure the target is within range
        if (aoeType == aoeTypes.circle && !possibleSpaces.Contains(target))
        {
            aoeTargetTile = source.Owner.Space;
        }

        // TODO: Decide targets based on what would be within the AOEs
        targets.Clear();

        // Update the possible targets based on the new target tile
        UpdatePossibleTargets();

        // Target all tiles in the area
        targets = possibleTargets;

        // Update the list of creatures in that area
        aoeTilesWithCreature = source.LevelSpawnerRef.TilesWithCreatures(targets);

        // Update list of creature targets
        creatureTargets = source.LevelSpawnerRef.CreaturesInList(AOETilesWithCreatures);
    }

    public override void UpdatePossibleTargets()
    {
        base.UpdatePossibleTargets();

        // Reset variables
        possibleTargets.Clear();
        possibleSpaces.Clear();

        // Get a list of every tile within range of the circle explosion
        // Only circle uses possibleSpaces
        if (aoeType == aoeTypes.circle)
        {
            possibleSpaces = source.LevelSpawnerRef.LineOfSight(
                source.LevelSpawnerRef.TilesInRange(source.Owner, range + 0.5f), source.Owner, true);
        }

        // Do nothing if the target tile is the player's space (resting state) unless its a circle with a range of 0
        if (aoeTargetTile == source.Owner.Space && !(aoeType == aoeTypes.circle && range == 0)) // We should get no target
        {
            return;
        }

        // Switch through each possible aoeType
        switch (aoeType)
        {
            case aoeTypes.none:
                Debug.Log("How the fuck did this happen");
                break;

            // Circle AOE
            case aoeTypes.circle:

                // The AOE originates from the target tile
                origin = aoeTargetTile;

                // Get every tile within range of the explosion
                possibleTargets = source.LevelSpawnerRef.LineOfSight(
                    source.LevelSpawnerRef.TilesInRange(origin, aoeReach), aoeTargetTile, true);


                break;

            case aoeTypes.cone:

                // Get every tile within range of the cone originating from the caster
                possibleTargets = source.LevelSpawnerRef.LineOfSight(
                    source.LevelSpawnerRef.TilesInCone(origin, aoeTargetTile, range, aoeAngle), 
                    source.Owner, true);

                break;

            case aoeTypes.line:

                // Get every tile within range of the line, originating from the player
                possibleTargets = source.LevelSpawnerRef.LineOfSight(
                    source.LevelSpawnerRef.TilesInLine(origin, aoeTargetTile, range, aoeReach),
                    source.Owner, true);

                break;
        }
    }

    public override void DoAction()
    {
        base.DoAction();

        // Rotate to face the center of the AOE, not the first target
        source.Owner.RotateToFaceTile(aoeTargetTile);
    }

    public override void CheckTargetsStillInRange()
    {
        // This function already covers all the logic of making sure people are still in range and within line of sight
        SetTarget(aoeTargetTile);
    }

    protected override string FormatRangeText()
    {
        string text = "AOE attack. ";

        // Have different text depending on the type
        switch (aoeType)
        {
            case aoeTypes.circle:
                text += aoeReach + " tile circle. " + range + " tile range. ";
                break;

            case aoeTypes.cone:
                if (range < 100) // has limited range
                {
                    text += range + " tile cone. " + aoeAngle + " degrees. ";
                }
                else // has unlimited range
                {
                    text += "Cone. " + aoeAngle + " degrees. ";
                }
                
                break;

            case aoeTypes.line:
                if (range < 100) // has limited range
                {
                    text += range + " tile line. " + aoeReach + " tile width. ";
                }
                else // has unlimited range
                {
                    text += "Line. " + aoeReach + " tile width. ";
                }
                break;
        }

        return text;
    }
}
