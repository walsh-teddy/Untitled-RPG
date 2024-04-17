using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : Action
{
    protected float speed = 0; // How far this can go. Assigned in the constructor

    // Variables passed in to the constructor
    protected int speedChange; // Either the new speed (ignoring the owner's) or the change to the owner's speed (depending on speedOverwrite)
    protected bool speedOverwrite; // If true, the speed change will become the new speed. Else it will add onto the owner's speed

    // Used for dijkstra searching (creating 1 and reusing it to avoid memory leaks)
    List<checkedTile> openList = new List<checkedTile> { };
    List<checkedTile> closedList = new List<checkedTile> { };
    List<checkedTile> foundPath = new List<checkedTile> { };
    checkedTile currentCheckedTile;
    // Wrapper used in dijkstra searching
    protected class checkedTile
    {
        public Tile tile; // The tile being looked at
        public checkedTile beforeTile; // What tile came before this
        public float length; // The distance from the start to get here
        public int step; // The amount of moves that have come before this

        public checkedTile(Tile tile, checkedTile beforeTile, float length, int step)
        {
            this.tile = tile;
            this.beforeTile = beforeTile;
            this.length = length;
            this.step = step;
        }
    }

    // Temp variables used in movement and animations
    protected float currentMoveLength = 0;

    // Properties
    public Tile CurrentTile
    {
        get { return targets[targets.Count - 1]; }
    }

    // Default Constructor
    public Move()
        : base("Move", 0, 0, 0, 0, false, Game.phase.move)
    {
        speedChange = 0;
        speedOverwrite = false;
        isMove = true;

        canSelectSpaces = true;
    }

    // Constructor for special types of move
    public Move(string displayName, int rechargeCost, int cooldownCost, int energyCost, int castTimeCost, bool isMinorAction, Game.phase phase, int speedChange, bool speedOverwrite)
    : base(displayName, rechargeCost, cooldownCost, energyCost, castTimeCost, isMinorAction, phase)
    {
        this.speedChange = speedChange;
        this.speedOverwrite = speedOverwrite;
        isMove = true;

        canSelectSpaces = true;
    }

    // Called by game.cs multiple times (possibly more than the move)
    public override void PlayAnimation()
    {
        // Don't do anything if there are no tiles left
        // TODO: end the move early if its open for now while movement collision isn't programmed yet
        if (currentTileIndex >= targets.Count || !Targets[currentTileIndex].IsOpen) // || !source.Owner.MovingThisPhase) // There are no tiles left or the move was interrupted
        {
            moveFinished = true;
            source.Owner.MovingThisPhase = false;
            return;
        }

        // Tell the creature to move to the next tile
        source.Owner.StartMove(Targets[currentTileIndex]);

        // Incriment the index up to the next tile
        currentTileIndex += 1;
    }

    // Check if this move will collide with any other moves
    public override void CheckCollision(List<Creature> allCreatures)
    {
        Debug.Log(displayName + " from " + source.Owner.DisplayName + " CheckCollision() called at index " + currentTileIndex);
        // TODO: Have a chance to push stationary targets back

        if (currentTileIndex == 1)
        {
            source.Owner.MovingThisPhase = true;
        }

        // Don't do anything if this move is already finished
        if (moveFinished || !source.Owner.MovingThisPhase) // This is not going to be moving this step
        {
            Debug.Log("Move already stopped");
            return;
        }
        foreach (Creature creature in allCreatures)
        {
            // Make sure this isn't looking at itself
            if (creature == source.Owner) // It is looking at itself
            {
                Debug.Log("Move looking at itself");
                continue;
            }

            // Test if they will go to the same spot
            if (StepAtIndex(currentTileIndex) == creature.PlannedMovement[1][currentTileIndex]) // They are going to collide
            {
                // If the other move is finished, this one automatically stops moving
                // TODO: Make pushing stationary targets possible
                if (!creature.MovingThisPhase)
                {
                    // Interrupt this move
                    source.Owner.MovingThisPhase = false;
                }
                else // Both of them are moving
                {
                    // Have them roll against eachother
                    Creature winner = source.GameRef.StatTest(source.Owner, creature, Game.stats.strength);

                    // Test who won
                    if (winner == source.Owner) // This creature won
                    {
                        // Interrupt the other move
                        creature.MovingThisPhase = false;
                    }
                    else if (winner == creature) // The other creature won
                    {
                        // Interrupt this move
                        source.Owner.MovingThisPhase = false;
                    }
                    else // It was a tie
                    {
                        // Interrupt both moves
                        creature.MovingThisPhase = false;
                        source.Owner.MovingThisPhase = false;
                    }
                }
            }
        }
    }

    // Add a tile to the list if its a valid move
    public override void SetTarget(Tile target)
    {
        // Check if the tile is imediately next to them
        if (possibleTargets.Contains(target)) // It is imediately next to the player
        {
            // Add the tile to the list
            currentMoveLength += CurrentTile.ConnectionLength(target);
            targets.Add(target);
        }
        else // It was not in possibleTargets
        {
            // Go back through the path to get here and add each tile in reverse order
            
            // Find the tile in the closed list (saved in currentCheckedTile to reuse a variable)
            foreach (checkedTile tile in closedList)
            {
                if (tile.tile == target) // This is the tile we're looking for
                {
                    // Save this tile
                    currentCheckedTile = tile;
                    break;
                }
            }

            // Add each tile in the path to a list (it will be backwards)
            foundPath.Clear();
            while (currentCheckedTile != null)
            {
                // Add this to a list to be read backwards later
                foundPath.Add(currentCheckedTile);

                // Move backwards one step
                currentCheckedTile = currentCheckedTile.beforeTile;
            }


            // Go throuh the foundPath list backwards to add each tile in the right order to the move list
            for (int i = foundPath.Count - 2; i >= 0; i--)
            {
                // Check to make sure the tile is within speed distance
                if (IsMoveWithinSpeed(foundPath[i]))
                {
                    targets.Add(foundPath[i].tile);
                }
            }

            // Update the current move length (the final length calculated at the end of foundPath (or begining, rather) is the total length
            currentMoveLength = foundPath[0].length;
        }

        // Update the list of valid next steps (just update it here rather than calculating every frame)
        UpdatePossibleTargets();
    }

    public bool IsMoveLegal(Tile target, Tile current)
    {
        // Test if they're not connected
        if (!target.IsConnected(current)) // It is not connected
        {
            return false;
        }

        // Test if its the same tile
        if (current == target)
        {
            //Debug.Log("Illegal move. Same tile");
            return false;
        }

        // Test if its occupied by an obstacle (creatures can move / be pushed)
        if (target.HasObstacle) // There is an obstacle in the way
        {
            //Debug.Log("Illegal move. Space is occupied");
            return false;
        }

        // Test if its too high
        if (target.Height - current.Height > source.Owner.StepHeight)
        {
            //Debug.Log("Illegal move. Too high of a step");
            return false;
        }

        // If it has not returned false yet, then its a legal move, so return true
        return true;
    }

    protected bool IsMoveWithinSpeed(checkedTile tile)
    {
        return (int)tile.length <= speed;
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
        // Dijkstra search through every tile until there are no available tiles unsearched within range

        // Clear the old lists
        possibleSpaces.Clear();
        openList.Clear();
        closedList.Clear();
        currentCheckedTile = null;

        // Start the open list with the starting tile
        openList.Add(new checkedTile(CurrentTile, null, currentMoveLength, targets.Count - 1));

        // Loop until every possible tile has been checked
        while (openList.Count > 0)
        {
            // The tile currently being looked at will be the tile this loop is looking at
            currentCheckedTile = openList[0];
            openList.Remove(currentCheckedTile);

            // Loop through each of this tiles connections
            foreach (Tile.TileConnection connection in currentCheckedTile.tile.Connections)
            {
                bool valid = true; // Allow this to be proven false
                bool alreadyFound = false;

                // Make sure the move is legal
                if (!IsMoveLegal(connection.tile, currentCheckedTile.tile)) // The move is not legal
                {
                    valid = false;
                }

                // Make sure no allies plan on being in this tile at this step
                if (source.Owner.TeamManager.PlannedMovementAtStep(currentCheckedTile.step + 1, phase, source.Owner).Contains(connection.tile)) // An ally plans on being here at this step
                {
                    valid = false;
                }

                // Make sure this tile is not already in the open list
                foreach (checkedTile tile in openList)
                {
                    if (tile.tile == connection.tile) // This tile is in the open list
                    {
                        // Check if this current path has a shorter length than the one we just found
                        if (tile.length > currentCheckedTile.length + connection.length && valid) // We just found a shorter path and this path is still valid otherwise
                        {
                            // Update the currently stored path
                            tile.length = currentCheckedTile.length + connection.length;
                            tile.beforeTile = currentCheckedTile;
                            tile.step = currentCheckedTile.step + 1;

                            // Still mark it as not valid since we don't want to add a new path to the closed list
                            valid = false;
                        }

                        alreadyFound = true;
                    }
                }

                // Make sure this tile is not already in the closed list
                foreach (checkedTile tile in closedList)
                {
                    if (tile.tile == connection.tile) // This tile is in the closed list
                    {
                        // Check if this current path has a shorter length than the one we just found
                        if (tile.length > currentCheckedTile.length + connection.length && valid) // We just found a shorter path and this path is still valid otherwise
                        {
                            // Update the currently stored path
                            tile.length = currentCheckedTile.length + connection.length;
                            tile.beforeTile = currentCheckedTile;
                            tile.step = currentCheckedTile.step + 1;

                            // Still mark it as not valid since we don't want to add a new path to the closed list
                            valid = false;
                        }

                        alreadyFound = true;
                    }
                }

                // If the tile is valid, add it to the open list
                if (valid && !alreadyFound) // This tile is valid and is not already in the open or closed list
                {
                    // Add this to the open list (and create a checkedTile wrapper)
                    openList.Add(new checkedTile(connection.tile, currentCheckedTile, currentCheckedTile.length + connection.length, currentCheckedTile.step + 1));
                }
            }

            // This tile has been fully checked so move it to the closed list
            closedList.Add(currentCheckedTile);
        }

        // Add to possible spaces and targets
        possibleTargets.Clear();
        possibleSpaces.Clear();
        // Loop through the closed list and add everything that is within 1 step
        foreach (checkedTile tile in closedList)
        {
            // If its within range, add it to possible spaces
            if (IsMoveWithinSpeed(tile) && tile.step != targets.Count - 1) // This tile is within range and is not the starting tile
            {
                // LIGHT HIGHLIGHT
                possibleSpaces.Add(tile.tile);

                // MEDIUM HIGHLIGHT
                // Also check if its within 1 step
                if (tile.step == targets.Count) // Its within 1 step
                {
                    possibleTargets.Add(tile.tile);
                }
            }
        }
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

        currentMoveLength = 0;

        // Calculate new targets
        UpdatePossibleTargets();
    }

    public override void EndTurn()
    {
        base.EndTurn();
        currentTileIndex = 1;
        moveFinished = false;
    }

    public override string FormatDescription(bool playerExists)
    {
        string text = "";

        if (playerExists) // This is being done in a game
        {
            if (speed != 1) // Plural tiles
            {
                text += "Move " + speed + " tiles. ";
            }
            else // Singular tile
            {
                text += "Move 1 tile. ";
            }
        }
        else // This is being done in a menu
        {
            if (speedOverwrite) // SpeedChange is the new set speed
            {
                if (speedChange != 1) // Plural tiles
                {
                    text += "Move " + speedChange + " tiles. ";
                }
                else // Singular tile
                {
                    text += "Move 1 tile. ";
                }
            }
            else // SpeedChange is an adition to source.owner.speed
            {
                text += "Move speed";

                if (speedChange > 0) // There is a speed bonus
                {
                    text += " +" + speedChange;
                }
                else if (speedChange < 0) // There is a speed penalty
                {
                    text += " " + speedChange;
                }

                text += " tiles. ";
            }
        }

        return text;
    }
}