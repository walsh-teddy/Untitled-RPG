using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager
{
    // Reference variables
    protected LevelSpawner levelSpawner;
    protected Game game;

    // Stored variables
    protected List<Creature> teamMembers = new List<Creature> { };
    public List<Creature> TeamMembers
    {
        get { return teamMembers; }
    }

    protected string teamName;
    protected bool teamReady = false;
    public bool TeamReady
    {
        get { return teamReady; }
    }

    // Cached variables
    protected List<Tile> plannedMovementAtThisStep = new List<Tile> { }; // Chached for use in PlannedMovementAtStep() without needing to create a new list each time
    protected List<Creature> enemies = new List<Creature> { };
    public List<Creature> Enemies
    {
        get
        {
            // Reset enemies list (cached so you don't have to create a new list every time)
            enemies.Clear();

            // Loop through all other teams in game.cs
            foreach (TeamManager team in game.Teams.Values)
            {
                // Make sure its not this team
                if (team != this) // It is not this team
                {
                    // Add each creature on it to the list
                    foreach (Creature enemy in team.TeamMembers)
                    {
                        enemies.Add(enemy);
                    }
                }
            }

            return enemies;
        }
    }


    public TeamManager(string teamName)
    {
        this.teamName = teamName;

        levelSpawner = GameObject.FindGameObjectWithTag("GameManager").GetComponent<LevelSpawner>();
        game = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Game>();
    }

    public virtual void AddTeamMember(Creature teamMember)
    {
        // Add this creature to the list
        teamMembers.Add(teamMember);

        // Record within the creature that this is its team manager
        teamMember.TeamManager = this;
    }

    public virtual void RemoveTeamMember(Creature teamMember)
    {
        teamMembers.Remove(teamMember);
    }

    public List<Tile> PlannedMovementAtStep(int stepIndex, phase phase, Creature ignoreThisCreature)
    {
        // Get an empty list
        plannedMovementAtThisStep.Clear();

        // Loop through each player
        foreach (Creature teamMember in teamMembers)
        {
            // Make sure we're not looking at the creature we should be ignoring
            // Only check ignoreThisCreature if a value was entered
            if (ignoreThisCreature != null) // There was a creature entered for ignoreThisCreature
            {
                if (teamMember == ignoreThisCreature) // We are currently looking at that creature and should stop
                {
                    // Ignore this creature and move onto the next
                    continue;
                    
                }
            }

            plannedMovementAtThisStep.Add(teamMember.PlannedMovementAtStep(stepIndex, phase));

/*            // Get their planned movement at this step
            // Test if their planned movement reaches this step
            if (teamMember.PlannedMovement[index].Count - 1 >= step) // Their planned movement has data for this step
            {
                // Record their movement for this step
                plannedMovementAtThisStep.Add(teamMember.PlannedMovement[index][step]);
            }
            else // Their planned movement ends before this step
            {
                // Use their last step (since that will be the tile they end on and will be there by this step)
                plannedMovementAtThisStep.Add(teamMember.PlannedMovement[index][teamMember.PlannedMovement[index].Count - 1]);
            }*/
        }

        return plannedMovementAtThisStep;
    }

    public List<Tile> PlannedMovementAtStep(int step, phase phase)
    {
        return PlannedMovementAtStep(step, phase, null);
    }

    public void UpdatePossibleTargets()
    {
        // Loop through every creature in the team and tell them to update all possible targets
        foreach (Creature teamMember in teamMembers)
        {
            teamMember.UpdatePossibleTargets();
        }
    }

    public void ReadyUp()
    {
        // Mark this team as ready
        teamReady = true;

        // Tell the game this team is ready
        game.TeamReady();
    }

    public void EndTurn()
    {
        // Reset teamReady
        teamReady = false;

        // Tell each team member to call EndTurn()
        foreach (Creature teamMember in teamMembers)
        {
            teamMember.EndTurn();
        }
    }
}
