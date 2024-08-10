using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Creature
{
    // TODO: Add variables for decision making
    [Header("AI Variables")]
    [SerializeField] protected float agressiveHealthThreshhold; // At what percentage of health will this creature stop being agressive
    [SerializeField] protected float preferedAgressiveDistance; // How far away this creature wants to be from its enemies when being agressive
    [SerializeField] protected float preferedDefensiveDistance; // How far away this creature wants to be from its enemies when being defensive
    List<Action> agressiveActions = new List<Action> { }; // Ordered in terms of preference
    List<Action> defensiveActions = new List<Action> { }; // Ordered in terms of preference
    List<Action> repositionActions = new List<Action> { }; // Ordered in terms of preference

    [SerializeField] float targetHealthWeight;
    [SerializeField] float targetEnergyWeight;

    public override void Create(Tile space)
    {
        base.Create(space);

        // Save their prefered agressive actions
        agressiveActions.Add(GetActionSourceByName("Longsword (2 handed)").GetActionbyName("Slash"));
        agressiveActions.Add(GetActionSourceByName("Longsword (2 handed)").GetActionbyName("Stab"));

        // Save their prefered defensive actions (unused as of now)
        defensiveActions.Add(GetActionSourceByName("Longsword (2 handed)").GetActionbyName("Block"));

        // Save their prefered reposition actions
        repositionActions.Add(GetActionSourceByName("Self").GetActionbyName("Dash"));
        repositionActions.Add(GetActionSourceByName("Self").GetActionbyName("Move"));
    }

    public override void AI()
    {
        // TODO: This is only the AI for the basic melee enemy and will not apply to all different types

        // Decide whether to be agressive or defensive
        // TODO: Only doing agressive actions right now for the sake of testing
        if (true) //(health > maxHealth * agressiveHealthThreshhold) // Health is high enough to be agressive
        {
            // Loop through each action and see if there are valid targets
            foreach (Action action in agressiveActions)
            {
                if (action.PossibleTargets.Count > 0 && action.Playable) // This action has valid targets and is playable
                {
                    //Debug.Log(action.DisplayName + " is playable with " + action.PossibleTargets.Count + " possible targets");
                    // Choose a target
                    action.SetTarget(PreferedTarget(action));

                    // Submit this action
                    SubmitAction(action);

                    // TODO: Instead have each enemy mark when they are ready and have the team manager ReadyUp once all AI are ready
                    teamManager.ReadyUp();

                    // Break out of the function
                    return;
                }
            }

            // If we got here, then there are no valid targets for agressive actions
            
            // Move towards the nearest enemy
            foreach (Action action in repositionActions)
            {
                // Cast to a move
                Move move = (Move)action;

                // Make sure the action is playable (Dash costs energy)
                if (move.Playable)
                {
                    // Set the target
                    move.SetTarget(levelSpawner.NearestCreature(teamManager.Enemies, this).Space);

                    // Submit the action
                    move.Chasing = true;
                    SubmitAction(move);

                    // TODO: Instead have each enemy mark when they are ready and have the team manager ReadyUp once all AI are ready
                    teamManager.ReadyUp();

                    // Break out of the function
                    return;
                }
            }
        }
        else // Health is too low. Be defensive
        {
            // TODO: Do this later
        }

        // TODO: Also add a minor action possibly (that would require not doing return when an action is submitted
    }

    protected virtual Tile PreferedTarget(Action action)
    {
        List<Creature> possibleTargets = levelSpawner.CreaturesInList(action.PossibleTargets);
        float highestTargetScore = -99999999999999; // Can go below 0 (starting arbitrarily low)
        Creature currentTarget = null;

        // Loop through each creature and record them if they have the highest value
        foreach (Creature creature in possibleTargets)
        {
            // Calculate the target score for this creature
            float targetScore = 0;

            // The score should be lower if their health is higher
            targetScore -= creature.Health * targetHealthWeight;

            // The score should be lower if their energy is high
            targetScore -= creature.Energy * targetEnergyWeight;

            // TODO: Consider the following as well
            //  - Available defensive actions
            //  - Maybe position
            //  - Is casting something

            // If this score is higher than before (or the first) then record this as the prefered target
            if (targetScore > highestTargetScore) // This is better than the previous record
            {
                highestTargetScore = targetScore;
                currentTarget = creature;
            }
        }

        return currentTarget.Space;
    }
    public override void SubmitAction(Action action)
    {
        //Debug.Log(displayName + " submitted " + action.DisplayName);
        base.SubmitAction(action);
    }

}
