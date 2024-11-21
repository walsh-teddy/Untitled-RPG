using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttack : Attack
{
    // Variables
    protected bool circleCenterIgnoreLineOfSight;
    protected aoeTypes aoeType;
    protected float aoeReach; // The radius of a circle and the width of a line
    protected float aoeAngle; // How wide a cone will be (only used by cone)
    protected float aoeHeight; // How high a circle will explode from the ground (only used by circles)
    protected Tile aoeTargetTile; // The center of a circle, or the target point of a line or cone
    protected List<Tile> aoeTilesWithCreature = new List<Tile> { };
    protected bool canBeBlockedByMelee; // True if this is a melee attack that can hit multiple targets, and so it can be blocked normally

    // Properties
    public override List<Tile> AOETilesWithCreatures
    {
        // Overriden in AOEAttack.cs
        get { return aoeTilesWithCreature; }
    }
    public override bool CanBeBlockedByMelee
    {
        get { return canBeBlockedByMelee; }
    }

    // Constructor that uses a data object
    public AOEAttack(AOEAttackData data)
        : base (data)
    {
        // Extract data from the data object
        aoeType = data.aoeType;
        aoeReach = data.aoeReach;
        aoeAngle = data.aoeAngle;
        aoeHeight = data.aoeHeight;
        circleCenterIgnoreLineOfSight = data.circleCenterIgnoreLineOfSight;
        canBeBlockedByMelee = data.canBeBlockedByMelee;

        // Default values
        actionType = actionType.aoeAttack;
        targetType = targetTypes.aoe;

        // Circle AOEs originate from their own point
        if (aoeType == aoeTypes.circle)
        {
            originatesFromAttacker = false;
        }

        // Default value
        actionType = actionType.aoeAttack;
    }

    // Mark the center tile instead of the actual targets of the attacks
    public override void SetTarget(Tile target)
    {
        // Do nothing if targets are locked
        if (targetsLocked) // Targets are locked
        {
            return;
        }

        // Set the target tile
        aoeTargetTile = target;

        // If its a circle AOE, make sure the target is within range
        if (aoeType == aoeTypes.circle && !possibleSpaces.Contains(target) && source != null)
        {
            aoeTargetTile = source.Owner.Space;
        }

        // TODO: Decide targets based on what would be within the AOEs
        targets.Clear();

        // Update the possible targets based on the new target tile
        UpdatePossibleTargets();

        // Target all tiles in the area
        targets = possibleTargets;

        // Break out if there is no source (only really happens if this is a brush)
        if (source == null)
        {
            return;
        }

        // Update the list of creatures in that area
        aoeTilesWithCreature = source.LevelSpawnerRef.TilesWithCreatures(targets);

        // Update list of creature targets
        creatureTargets = source.LevelSpawnerRef.CreaturesInList(AOETilesWithCreatures);
    }

    public override void UpdatePossibleTargets()
    {
        SetUpVariables();

        // Don't do anything if the target is already locked
        if (targetsLocked)
        {
            // Still update the list of targets
            targets = possibleTargets;
            aoeTilesWithCreature = source.LevelSpawnerRef.TilesWithCreatures(targets);
            creatureTargets = source.LevelSpawnerRef.CreaturesInList(AOETilesWithCreatures);
            return;
        }

        // Reset variables
        possibleTargets.Clear();
        possibleSpaces.Clear();

        // Get a list of every tile within range of the circle explosion
        // Only circle uses possibleSpaces
        if (aoeType == aoeTypes.circle)
        {
            // Get all spaces within range
            possibleSpaces = source.LevelSpawnerRef.TilesInRange(source.Owner, range + 0.5f, 1, false);
            if (!circleCenterIgnoreLineOfSight) // The center of the target needs line of sight
            {
                possibleSpaces = possibleSpaces = source.LevelSpawnerRef.LineOfSight(possibleSpaces, source.Owner, true);
            }
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
                Debug.LogError("Calling UpdatePossibleTargets() for an AOE attack with aoeType=none");
                break;

            // Circle AOE
            case aoeTypes.circle:

                // The AOE originates from the target tile
                origin = aoeTargetTile;

                // Get every tile within range of the explosion
                possibleTargets = source.LevelSpawnerRef.TilesInRange(origin, aoeReach, aoeHeight, 0, true);
                if (!ignoreLineOfSight) // The explosion needs line of sight
                {
                    possibleTargets = source.LevelSpawnerRef.LineOfSight(possibleTargets, origin, true);
                }

                break;

            case aoeTypes.cone:

                // Get every tile within range of the cone originating from the caster
                possibleTargets = source.LevelSpawnerRef.TilesInCone(origin, aoeTargetTile, range, aoeAngle);
                if (!ignoreLineOfSight) // The explosion needs line of sight
                {
                    possibleTargets = source.LevelSpawnerRef.LineOfSight(possibleTargets, origin, true);
                }

                break;

            case aoeTypes.line:

                // Get every tile within range of the line, originating from the player
                possibleTargets = source.LevelSpawnerRef.TilesInLine(origin, aoeTargetTile, range, aoeReach);
                if (!ignoreLineOfSight) // The explosion needs line of sight
                {
                    possibleTargets = source.LevelSpawnerRef.LineOfSight(possibleTargets, origin, true);
                }

                break;
        }
    }

    public override void PlayAnimation()
    {
        // Play the animation (and rotate, but overwrite that)
        base.PlayAnimation();

        // Face the center of the AOE (overwriting whatever was rotated before)
        source.Owner.RotateToFaceTile(aoeTargetTile);

        // TODO: See about particle systems
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
                text += (int)aoeReach + " tile circle. " + (int)range + " tile range. ";
                if (circleCenterIgnoreLineOfSight)
                {
                    text += "Target ignores line of sight. ";
                }
                break;

            case aoeTypes.cone:
                if (range < 100) // has limited range
                {
                    text += (int)range + " tile cone. " + aoeAngle + " degrees. ";
                }
                else // has unlimited range
                {
                    text += "Cone. " + aoeAngle + " degrees. ";
                }
                
                break;

            case aoeTypes.line:
                if (range < 100) // has limited range
                {
                    text += (int)range + " tile line. " + (int)aoeReach + " tile width. ";
                }
                else // has unlimited range
                {
                    text += "Line. " + aoeReach + " tile width. ";
                }
                break;
        }

        if (ignoreLineOfSight)
        {
            text += "Ignores line of sight. ";
        }

        return text;
    }

    public override void Discard()
    {
        base.Discard();
        aoeTilesWithCreature.Clear();
    }

    public override void SetUpVariables()
    {
        base.SetUpVariables();
        if (aoeTargetTile == null)
        {
            aoeTargetTile = origin;
        }
    }
}
