using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : Action
{
    protected int speed = 0; // How far this can go. Assigned in the constructor

    // Variables passed in to the constructor
    protected int speedChange; // Either the new speed (ignoring the owner's) or the change to the owner's speed (depending on speedOverwrite)
    protected bool speedOverwrite; // If true, the speed change will become the new speed. Else it will add onto the owner's speed

    // Temp variables used in movement and animations
    protected float currentMoveLength = 0;
    protected bool moveFinished = false; // Set to true when this has moved its final step this turn
    protected int currentTileIndex = 1; // Used when performing the move animations. Always starts on 1 because index 0 is the starting tile (and we don't move to that)
    protected bool chasing = false; // If true, select a creature instead of a path of tiles, and pathfind to them every step. Use creatureTargets[0]
    protected float currentChaseLength = 0; // Used when chasing to know how far its moved
    protected bool pausedLastStep = false; // Used for chasing. If the move paused 2 steps in a row, then its done

    // Used for dijkstra searching (creating 1 and reusing it to avoid memory leaks)
    List<CheckedTile> openList = new List<CheckedTile> { };
    List<CheckedTile> closedList = new List<CheckedTile> { };
    List<CheckedTile> foundPath = new List<CheckedTile> { };
    CheckedTile currentCheckedTile;

    // Used for external collision calculations in Game.cs
    Tile currentMoveToTile; // The tile the move is moving to this step
    Tile previousMoveToTile; // The tile the move moved to last step

    // TODO: Add movement interruptions
    protected Creature moveContestion; // Which creature this move would bump into this step. Assigned by game.cs

    // Properties
    public Tile CurrentTile
    {
        get { return targets[targets.Count - 1]; }
    }
    public int CurrentTileIndex
    {
        get { return currentTileIndex; }
    }
    public bool MoveFinished
    {
        get { return moveFinished; }
    }
    public int TotalSteps
    {
        get
        {
            if (targets.Count - 1 <= speed)
            {
                return targets.Count - 1;
            }
            else
            {
                return speed;
            }
        }
    }
    public bool Chasing
    {
        get { return chasing; }
        set 
        {
            // Don't do anything if the targets are locked
            if (targetsLocked) // The targets are locked
            {
                return;
            }

            // Update the value
            chasing = value;

            // Update values based on the new value for chasing
            if (chasing) // It was switched to chasing mode
            {
                targetType = targetTypes.single;
                
            }
            else // It was switched to normal mode
            {
                targetType = targetTypes.move;
            }
           
            // Update possible targets
            targets.Clear();
            creatureTargets.Clear();
            UpdatePossibleTargets();
        }
    }
    public Tile CurrentMoveToTile
    {
        get { return currentMoveToTile; }
    }
    public Tile PreviousMoveToTile
    {
        get { return previousMoveToTile; }
    }

    // Constructor that uses a data object
    public Move(MoveData data)
        : base(data)
    {
        // Extract data from data object
        speedChange = data.speedChange;
        speedOverwrite = data.speedOverwrite;

        // Default values
        actionType = actionType.move;
        targetType = targetTypes.move;
    }

    // Called by game.cs multiple times (possibly more than the move)
    // This is the function that actually updates the position of the owner
    public override void PlayAnimation()
    {
        // Update moveToTiles
        previousMoveToTile = currentMoveToTile;

        // Look forward 1 step if the move is not already done
        // Do different things depending on if its chasing or not
        if (!chasing && !moveFinished && source.Owner.MovingThisPhase) // Not chasing
        {
            // Check if this is the last step
            if (currentTileIndex <= TotalSteps) // This is not the last step
            {
                currentMoveToTile = Targets[CurrentTileIndex];
            }
            else // This is the last target
            {
                moveFinished = true;
            }
        }
        else if (chasing && !moveFinished && source.Owner.MovingThisPhase) // Chasing
        {
            // Look to the next tile in the pathfinding, and update currentChaseLength
            Tile nextStep = Chase(creatureTargets[0].Space);

            if ((int)currentChaseLength <= speed && nextStep != null) // This can go further and target is reachable
            {
                // Don't do anything if this would collide with the target
                if (nextStep == creatureTargets[0].Space) // This would collide with the target
                {
                    if (pausedLastStep) // This also would've collided with the target last step (they probably stopped moving)
                    {
                        // Stop the move
                        moveFinished = true;
                    }
                    else // This is the first time this move has had to pause
                    {
                        Debug.Log(source.Owner.DisplayName + " " + displayName + " would collide with target. Pausing for 1 step");
                        // Don't update currentMoveToTile
                        // Pause for 1 step 
                        pausedLastStep = true;
                    }
                }
                else // This would not collide with the target
                {
                    // Move to the next step
                    currentMoveToTile = nextStep;
                    pausedLastStep = false;
                }
            }
            else // This is the last step 
            {
                moveFinished = true;
            }
        }

        // Test if this is the last move
        // Don't do anything if there are no tiles left
        // TODO: end the move early if its open for now while movement collision isn't programmed yet
        if (moveFinished || // Was marked as done by an outside source
            !source.Owner.MovingThisPhase) // The owner was marked as not moving by an outside source (probably because of a collision)
        {
            // Don't move anymore
            moveFinished = true;
            source.Owner.MovingThisPhase = false;
            return;
        }
        // Tell the owner to move to this tile
        source.Owner.StartMove(currentMoveToTile);

        if (currentMoveToTile != source.Owner.Space) // The move is going somewhere new
        {
            // Play the actual animation
            base.PlayAnimation();
        }

        // Incriment the index up to the next tile (mainly used for non-chasing, but doing this in both cases for saftey)
        // CurrentMoveLength is updated in Chase()
        currentTileIndex += 1;
    }

    // Add a tile to the list if its a valid move
    public override void SetTarget(Tile target)
    {
        // Do nothing if targets are locked
        if (targetsLocked) // Targets are locked
        {
            // Do nothing
            return;
        }

        if (chasing) // They should select a creature to chase
        {
            creatureTargets.Clear();
            targets.Clear();
            // Make sure its a valid target
            if (possibleTargets.Contains(target)) // It is a valid target
            {
                // Save the creature in that space
                creatureTargets.Add(target.Occupant);

                // Also save the target space (mostly there to display in the UI)
                targets.Add(target);
            }
        }
        else if (possibleTargets.Contains(target)) // It is imediately next to the player
        {
            // Add the tile to the list
            currentMoveLength += CurrentTile.ConnectionLength(target);
            targets.Add(target);
        }
        else if (possibleSpaces.Contains(target)) // It was in possible spaces
        {
            // Go back through the path to get here and add each tile in reverse order
            
            // Find the tile in the closed list (saved in currentCheckedTile to reuse a variable)
            foreach (CheckedTile tile in closedList)
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
            // TODO: Have the weight that height difference adds to tiles be based on the creature. It should be [(target.Height - current.Height) / StepHeight] so if someone has a step height of 3, they can go up most hills without being impeeded
            //Debug.Log("Illegal move. Too high of a step");
            return false;
        }

        // If it has not returned false yet, then its a legal move, so return true
        return true;
    }

    protected bool IsMoveWithinSpeed(CheckedTile tile)
    {
        return (int)tile.length <= speed;
    }
    protected bool IsMoveWithinSpeed(CheckedTile tile, float startingLength)
    {
        return (int)(tile.length + startingLength) <= speed;
    }
    protected bool IsMoveWithinSpeed(TileConnection tile, float startingLength)
    {
        return (int)(tile.length + startingLength) <= speed;
    }

    public override void UpdatePossibleTargets()
    {
        SetUpVariables();

        // LIGHT HIGHLIGHT
        // Dijkstra search through every tile until there are no available tiles unsearched within range

        // Clear the old lists
        possibleSpaces.Clear();
        possibleTargets.Clear();
        openList.Clear();
        closedList.Clear();
        currentCheckedTile = null;

        // Leave lists empty if a new target can't be selected
        if (targetsLocked) // A new target cannot be selected
        {
            return;
        }

        // Add the starting tile to openList
        if (!chasing) // Not chasing, should start from wherever the most recent target was
        {
            // Start the open list with the starting tile
            openList.Add(new CheckedTile(CurrentTile, null, currentMoveLength, targets.Count - 1));
        }
        else // Is chasing. Always start from the owner's position
        {
            openList.Add(new CheckedTile(source.Owner.Space, null, 0, 1));
        }

        // Loop until every possible tile has been checked
        while (openList.Count > 0)
        {
            // The tile currently being looked at will be the tile this loop is looking at
            currentCheckedTile = openList[0];
            openList.Remove(currentCheckedTile);

            // Loop through each of this tiles connections
            foreach (TileConnection connection in currentCheckedTile.tile.Connections)
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
                foreach (CheckedTile tile in openList)
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
                foreach (CheckedTile tile in closedList)
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
                    openList.Add(new CheckedTile(connection.tile, currentCheckedTile, currentCheckedTile.length + connection.length, currentCheckedTile.step + 1));
                }
            }

            // This tile has been fully checked so move it to the closed list
            closedList.Add(currentCheckedTile);
        }

        // Add to possible spaces and targets

        // Loop through the closed list and add everything that is within 1 step
        foreach (CheckedTile tile in closedList)
        {
            // If its within range, add it to possible spaces
            if (IsMoveWithinSpeed(tile) && tile.step != targets.Count - 1) // This tile is within range and is not the starting tile
            {
                // LIGHT HIGHLIGHT
                possibleSpaces.Add(tile.tile);

                // MEDIUM HIGHLIGHT
                // Also check if its within 1 step (and this should select a tile, not a creature)
                if (tile.step == targets.Count && !chasing) // Its within 1 step
                {
                    possibleTargets.Add(tile.tile);
                }
            }
        }

        // Also find the space of all creatures if this move should be chasing
        if (chasing) // This move should be chasing
        {
            foreach (Creature creature in source.GameRef.Creatures)
            {
                // Can't select themself
                if (creature != source.Owner)
                {
                    possibleTargets.Add(creature.Space);
                }
            }
        }
    }

    // Called every UpdatePossibleTargets() because the owner's speed can change
    public override void SetUpVariables()
    {
        base.SetUpVariables();

        if (speedOverwrite) // The speed is overwritten
        {
            // Overwrite Owner.Speed as speedChange for this action's speed
            speed = speedChange;
        }
        else // The speed is not overwritten
        {
            // Record the owner's speed (accounting for any possible buffs this turn) plus any change to speed
            speed = source.Owner.Speed + source.Owner.ExpectedStatIncrease(buffableCreatureStats.speed) + speedChange;
        }

        if (targets.Count == 0)
        {
            targets.Add(source.Owner.Space);
        }
    }

    public override void Discard()
    {
        // Clear the list of tiles
        base.Discard();

        // Add the tile the player is standing on as the first tile in targets, if
        if (!targetsLocked)
        {
            targets.Add(source.Owner.Space);
            currentMoveLength = 0;
        }

        // Calculate new targets
        UpdatePossibleTargets();
    }

    public override void EndTurn()
    {
        base.EndTurn();
        currentTileIndex = 1;
        currentMoveLength = 0;
        currentChaseLength = 0;
        moveFinished = false;
        pausedLastStep = false;

        currentMoveToTile = source.Owner.Space;
        previousMoveToTile = source.Owner.Space;
    }

    public override string FormatDescription()
    {
        bool playerExists = source.Owner != null;
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

    // Return the tile that the owner should move to to be closer to the target its chasing
    public Tile Chase(Tile targetTile)
    {
        // LIGHT HIGHLIGHT
        // A* search to creatureTargets[0] (the creature this is trying to chase) and return the first step of that path

        // Clear the old lists
        possibleSpaces.Clear();
        possibleTargets.Clear();
        openList.Clear();
        closedList.Clear();
        currentCheckedTile = null;

        // Add the starting tile to openList (with the constructor that estimates a cost to get to the target tile)
        // Also start this with currentChaseLength, which is only used in chasing
        currentCheckedTile = new CheckedTile(source.Owner.Space, null, 0, 1, targetTile);

        // Add the imedaite connections from the starting tile, and only consider them if they are within range.
        // Don't bother checking if they have already been found (they have not)
        foreach (TileConnection connection in currentCheckedTile.tile.Connections)
        {
            bool valid = true; // Allow this to be proven false

            // Make sure the move is legal
            if (!IsMoveLegal(connection.tile, currentCheckedTile.tile)) // The move is not legal
            {
                valid = false;
            }

            // Make sure its within range
            if (!IsMoveWithinSpeed(connection, currentChaseLength)) // This step is too far
            {
                valid = false;
            }

            // If the tile is valid, add it to the open list
            if (valid) // This tile is valid and is not already in the open or closed list
            {
                // Add this to the open list (and create a checkedTile wrapper) (but also include the target tile, to get an estimated cost)
                openList.Add(new CheckedTile(connection.tile, currentCheckedTile, currentCheckedTile.length + connection.length, currentCheckedTile.step + 1, targetTile));
            }
        }

        closedList.Add(currentCheckedTile);

        // Loop until every possible tile has been checked
        while (openList.Count > 0)
        {
            // The tile currently being looked at will be the tile this loop is looking at
            // Select the tile with the lowest estimated length
            currentCheckedTile = openList[0]; // Start on the first tile in the list as a starting point
            // Find the tile in openList with the lowest expected cost to look at next
            foreach (CheckedTile tile in openList)
            {
                // Check if this tile has the lower cost
                if (tile.estimatedDist < currentCheckedTile.estimatedDist) // This tile is closer to the target
                {
                    currentCheckedTile = tile;
                }
            }

            // Remove this tile from the open list
            openList.Remove(currentCheckedTile);

            // See if this is the target tile
            if (currentCheckedTile.tile == targetTile) // This is the target!
            {
                int steps = 0;
                // Find the first step that leads to this path
                while (currentCheckedTile.step > 2)
                {
                    steps += 1;
                    // Go back until we find the 1st step outwards
                    currentCheckedTile = currentCheckedTile.beforeTile;
                }

                // Return this step
                currentChaseLength += currentCheckedTile.length;
                return currentCheckedTile.tile;
            }

            // Loop through each of this tiles connections
            foreach (TileConnection connection in currentCheckedTile.tile.Connections)
            {
                bool valid = true; // Allow this to be proven false
                bool alreadyFound = false;

                // Make sure the move is legal
                if (!IsMoveLegal(connection.tile, currentCheckedTile.tile)) // The move is not legal
                {
                    valid = false;
                }

                // Make sure this tile is not already in the open list
                foreach (CheckedTile tile in openList)
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
                foreach (CheckedTile tile in closedList)
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
                    // Add this to the open list (and create a checkedTile wrapper) (but also include the target tile, to get an estimated cost)
                    openList.Add(new CheckedTile(connection.tile, currentCheckedTile, currentCheckedTile.length + connection.length, currentCheckedTile.step + 1, targetTile));
                }
            }

            // This tile has been fully checked so move it to the closed list
            closedList.Add(currentCheckedTile);
        }

        // If we make it to this point and have not returned a value, then the target is unreachable or is out of range (normal)
        //Debug.LogError(displayName + " chase target is unreachable.");
        return null;
    }

    public override void CreateUI()
    {
        base.CreateUI();

        // Also create a chase button automatically for this
        GameObject.Instantiate(source.UIManagerRef.UIMoveChaseButtonPrefab, uiRoot.transform).GetComponent<uiMoveChaseButton>().Create(this);
    }

    /*    public virtual Tile StepAtIndex(int index)
        {
            // Test if the index is within range
            if (index <= targets.Count - 1 && index >= 0) // The index is within range
            {
                // Return the chosen index
                return targets[index];
            }
            else if (index < 0) // The index is below the min
            {
                // Return the index 0
                return targets[0];
            }
            else // The index is above the max
            {
                // Return the last index
                return targets[targets.Count - 1];
            }
        }*/
}