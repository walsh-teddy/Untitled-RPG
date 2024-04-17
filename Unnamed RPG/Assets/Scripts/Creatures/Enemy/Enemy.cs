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

        // Give them equipment
        activeActionSources.Add(new BastardSword1H(this));

        // Save their prefered agressive actions
        agressiveActions.Add(activeActionSources[1].ActionList[0]); // Bastard sword slash
        agressiveActions.Add(activeActionSources[1].ActionList[1]); // Bastard sword stab
        agressiveActions.Add(activeActionSources[0].ActionList[0]); // Self recover
        agressiveActions.Add(activeActionSources[0].ActionList[3]); // Self punch

        // Save their prefered defensive actions (unused as of now)
        defensiveActions.Add(activeActionSources[0].ActionList[1]); // Self recover
        defensiveActions.Add(activeActionSources[1].ActionList[2]); // Bastard sword block
        defensiveActions.Add(activeActionSources[1].ActionList[1]); // Bastard Sword block

        // Save their prefered reposition actions
        repositionActions.Add(activeActionSources[0].ActionList[2]); // Self Dash
        repositionActions.Add(activeActionSources[0].ActionList[1]); // Self Move
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

                    // Break out of the function
                    return;
                }
            }

            // If we got here, then there are no valid targets for agressive actions
            
            // Move towards the nearest enemy
            foreach (Action action in repositionActions)
            {
                // Make sure the action is playable (Dash costs energy)
                if (action.Playable)
                {
                    // Set the target
                    action.SetTarget(levelSpawner.NearestCreature(teamManager.Enemies, this).Space);

                    // Submit the action
                    SubmitAction(action);

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
}
