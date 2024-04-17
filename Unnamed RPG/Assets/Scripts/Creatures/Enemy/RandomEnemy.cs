using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEnemy : Enemy
{
    protected List<Action> availableActions = new List<Action> { };

    // Cached variables
    protected Action randomAction;
    protected Tile randomTarget;

    public override void Create(Tile space)
    {
        base.Create(space);

        // Create a list of every available action
        // Loop through every source
        foreach (ActionSource source in activeActionSources)
        {
            // Loop through every action in that source
            foreach (Action action in source.ActionList)
            {
                // Add the action to the list
                availableActions.Add(action);
            }
        }
    }
    public override void AI()
    {
        RandomAction();
    }

    // Randomly select an action and targets. If that is impossible for that action, repeat with a new random action
    protected void RandomAction()
    {
        // Randomly determine a number
        randomAction = availableActions[Random.Range(0, availableActions.Count - 1)];        

        // Test if that action can be done
        if (randomAction.Playable && // The action is playable
            (randomAction.PossibleTargets.Count > 0 || !randomAction.NeedsTarget) // The action has possible targets or doesn't need them
            ) // The action is available!
        {
            // Select a target (if it needs one)
            if (randomAction.NeedsTarget) // It needs a target
            {
                // If it is a move, consider possibleSpaces as well
                if (!randomAction.IsMove) // It is not a move
                {
                    // Only look at possible targets
                    randomTarget = randomAction.PossibleTargets[Random.Range(0, randomAction.PossibleTargets.Count - 1)];
                    randomAction.SetTarget(randomTarget);
                }
                else // It is a move
                {
                    // Look at possible spaces rather than possible targets
                    randomTarget = randomAction.PossibleSpaces[Random.Range(0, randomAction.PossibleSpaces.Count - 1)];
                    randomAction.SetTarget(randomTarget);
                }
            }

            // Submit the action
            SubmitAction(randomAction);
        }
        else // The action is not available
        {
            // Try again with a new random target
            RandomAction();
        }
    }
}
