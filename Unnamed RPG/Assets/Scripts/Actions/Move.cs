using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : Action
{
    // Atributes
    protected int speed = 0; // Normally just source.Owner.Speed. Can be 1 (in the case of step) or source.Owner.Speed + 2 in the case of dash
    protected int currentTileIndex = 1; // Always starts on 1 because index 0 is the starting tile (and we don't move to that)

    // Variables passed in to the constructor
    protected int speedChange; // Either the new speed (ignoring the owner's) or the change to the owner's speed (depending on speedOverwrite)
    protected bool speedOverwrite; // If true, the speed change will become the new speed. Else it will add onto the owner's speed

    // Default Constructor
    public Move()
        : base("Move", 0, 0, 0, 0, false, Game.phase.move)
    {
        speedChange = 0;
        speedOverwrite = false;
        isMove = true;
    }

    // Constructor for special types of move
    public Move(string displayName, int rechargeCost, int cooldownCost, int energyCost, int castTimeCost, bool isMinorAction, Game.phase phase, int speedChange, bool speedOverwrite)
    : base(displayName, rechargeCost, cooldownCost, energyCost, castTimeCost, isMinorAction, phase)
    {
        this.speedChange = speedChange;
        this.speedOverwrite = speedOverwrite;
        isMove = true;
    }

    // Called by game.cs multiple times (possibly more than the move)
    public override void DoAction()
    {

        // Don't do anything if there are no tiles left
        if (currentTileIndex >= targets.Count) // There are no tiles left
        {
            return;
        }

        // Tell the creature to move to the next tile
        source.Owner.StartMove(Targets[currentTileIndex]);

        // Incriment the index up to the next tile
        currentTileIndex += 1;
    }

    // Add a tile to the list if its a valid move
    public override void SetTarget(Tile target)
    {
        // Check if the player has any speed left
        if (targets.Count >= speed + 1) // +1 because the curentMoveList includes the starting tile
        {
            Debug.Log("Illegal move. Reached max speed");
            return;
        }

        // Check if the move is legal from the most recent point
        if (!possibleTargets.Contains(target))
        {
            return;
        }

        // Add the tile to the list
        targets.Add(target);

        // Update the list of valid next steps (just update it here rather than calculating every frame)
        UpdatePossibleTargets();
    }

    public bool IsMoveLegal(Tile target)
    {
        // Test if they have enough speed left (Count - 1 because it always includes the starting space)
        if (targets.Count - 1 >= speed)
        {
            return false;
        }

        // Test if they're not adjacent
        if (!source.LevelSpawnerRef.IsTileAdjacent(targets[targets.Count - 1], target))
        {
            return false;
        }

        // Test if its the same tile
        if (targets[targets.Count - 1] == target)
        {
            //Debug.Log("Illegal move. Same tile");
            return false;
        }

        // Test if its occupied
        if (!target.IsOpen)
        {
            //Debug.Log("Illegal move. Space is occupied");
            return false;
        }

        // Test if its too high
        if (target.Height - targets[targets.Count - 1].Height > source.Owner.StepHeight)
        {
            //Debug.Log("Illegal move. Too high of a step");
            return false;
        }

        // If it has not returned false yet, then its a legal move, so return true
        return true;
    }

    public override void UpdatePossibleTargets()
    {
        base.UpdatePossibleTargets();

        // This will only trigger at the very begining after the map is made
        if (targets.Count == 0)
        {
            SetUpVariables();
        }
        // LIGHT HIGHLIGHT
        // TODO: Light highlight every possible tile within remaining move speed

        // MEDIUM HIGHLIGHT
        // Clear the old list
        possibleTargets.Clear();
        
        // Loop through all adjasent tiles to the most recent move
        foreach (Tile tile in source.LevelSpawnerRef.AdjacentTiles(targets[targets.Count - 1]))
        {
            if (IsMoveLegal(tile))
            {
                possibleTargets.Add(tile);
            }
        }

        // No heavy highlight
    }

    // Called in UpdatePossibleTargets only if the targets list is empty (would only happen if this was the first time it was called)
    // This can't be called in the constructor because of reasons
    public virtual void SetUpVariables()
    {
        if (speedOverwrite) // The speed is overwritten
        {
            speed = speedChange;
        }
        else // The speed is not overwritten
        {
            speed = source.Owner.Speed + speedChange;
        }

        targets.Add(source.Owner.Space);
    }

    public override void Discard()
    {
        // Clear the list of tiles
        base.Discard();

        // Add the tile the player is standing on as the first tile in targets, always
        targets.Add(source.Owner.Space);

        // Calculate new targets
        UpdatePossibleTargets();
    }

    public override void EndTurn()
    {
        base.EndTurn();
        currentTileIndex = 1;
    }
}
