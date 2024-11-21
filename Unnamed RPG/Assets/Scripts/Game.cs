using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Game : MonoBehaviour
{
    // ATTRIBUTES
    protected static phase currentPhase;

    public List<ActionSource> thrownWeapons = new List<ActionSource> { };
    protected UIManager uiManager;
    protected LevelSpawner levelSpawner;
    protected PlayerManager playerManager; // Included in teamMangers
    protected Dictionary<string, TeamManager> teams = new Dictionary<string, TeamManager> { };
    protected Pointer pointer;
    [SerializeField] protected CameraFocus cameraFocus;
    protected List<Creature> creatures = new List<Creature> { }; // Only contains the living creatures
    protected List<Creature> deadCreatures = new List<Creature> { };
    protected List<Creature> creaturesToDecideAction = new List<Creature> { };
    protected static List<Action> actionStack = new List<Action> { };
    [SerializeField] public const int CLASH_ATTACK_WINDOW = 1; // If 2 attacks roll within [this] of eachother, then they clash

    // Lists of actions per phase
    protected List<Action> actionsThisPhase = new List<Action> { };
    protected List<CastAction> castsThisPhase = new List<CastAction> { };
    protected List<Attack> attacksThisPhase = new List<Attack> { };
    protected List<Move> movesThisPhase = new List<Move> { };
    protected List<BuffAction> buffsThisPhase = new List<BuffAction> { };

    [Header("Animation")]
    [SerializeField] protected float castAnimationTime = 1.5f; // Time (in seconds) that a cast action animation takes
    [SerializeField] protected float attackAnimationTime = 2; // Time (in seconds) that an attack animation takes
    [SerializeField] protected float buffAnimationTime = 1.5f; // Time (in seconds) that a buff animation takes
    [SerializeField] protected float moveAnimationTime = 1; // Time (in seconds) that a move animation takes
    [SerializeField] protected float animationPauseTime = 1; // Time (in seconds) in between animations
    [SerializeField] protected float moveAnimationPauseTime = 0.5f;

    // ATTACK PHASE LOGIC
    protected int currentAttackLineIndex = 0;
    protected List<AttackLine> attackLinesThisPhase = new List<AttackLine> { };
    protected List<Tile> possibleTargetsThisRound = new List<Tile> { };

    // MOVE PHASE LOGIC
    //protected int longestMove;
    protected int currentMoveStep = 1;

    // States
    protected gameState currentState = gameState.nothingSelected;

    // PROPERTY
    public gameState CurrentState
    {
        // Return the currentState
        get { return currentState; }

        // Update the curent state with some changes
        set
        {
            // Leave perpective mode every time the state changes
            cameraFocus.LeavePerspective();
            pointer.AOETargetLocked = false;

            // Update the new state
            currentState = value;

            // Do stuff depending on the new state
            switch (currentState)
            {
                case gameState.playerSelected:
                    // Unhighlight all tiles
                    levelSpawner.UnHighlightAllTiles();
                    break;
                case gameState.playerActionSourceSelectAction:
                    // Unhighlight all tiles
                    levelSpawner.UnHighlightAllTiles();
                    break;
                case gameState.playerActionSelectTarget:
                    // Highlight possible targets
                    levelSpawner.HighlightTiles(
                        playerManager.SelectedPlayer.SelectedAction.PossibleSpaces, // Light tiles
                        playerManager.SelectedPlayer.SelectedAction.PossibleTargets // Medium tiles
                    );
                    break;
            }

            // Update the UI in every case
            uiManager.UpdateUI();
        }
    }
    public phase CurrentPhase
    {
        get { return currentPhase; }
    }
    public List<Creature> Creatures
    {
        get { return creatures; }
    }
    public List<ActionSource> ThrownWeapons
    {
        get { return thrownWeapons; }
    }
    public Dictionary<string, TeamManager> Teams
    {
        get { return teams; }
    }
    public PlayerManager PlayerManager
    {
        get { return playerManager; }
    }

    // METHODS
    protected void Awake()
    {
        // Set up variables
        uiManager = gameObject.GetComponent<UIManager>();
        levelSpawner = gameObject.GetComponent<LevelSpawner>();
        pointer = gameObject.GetComponent<Pointer>();

        currentPhase = phase.PredictedDecision;

        // Put the player manager in the dictionary of team managers
        playerManager = new PlayerManager();
        teams.Add("player", playerManager);
    }
    protected void Start()
    {
        // Now that all the variables are set up (in Awake), push commands to other components
        levelSpawner.SpawnLevel();

        // Get initial targets for each action
        foreach (Creature creature in creatures)
        {
            creature.UpdatePossibleTargets();
        }

        // Create the UI elements for each action source and action
        uiManager.CreateUI();

        // Start the first phase (predicted decision)
        StartCoroutine(NextPhase());
    }
    
    // A new creature was created and should be added to the list
    public void NewCreature(Creature newCreature)
    {
        // Record this creature in a list
        creatures.Add(newCreature);

        // Assign it a team manager
        AddToTeam(newCreature);
    }


    // Most of the code for what happens in each phase (Prep, Attack, and Move are all automatic so every piece of
    // logic for them happens here)
    public void SubmitAction(Action action)
    {
        // Add the action to a list
        actionStack.Add(action);

        // TODO: Delete this
        //PrintSubmittedActions();
    }
    public void UnsubmitAction(Action action)
    {
        // Make sure this action is actually submitted
        if (actionStack.Contains(action)) // This action has been submitted
        {
            // Remove it
            actionStack.Remove(action);

            // TODO: Delete this
            //PrintSubmittedActions();
        }
        else // This action has not been submitted (something has gone wrong)
        {
            Debug.LogError("UnsubmitAction() called on invalid action");
        }
    }
    protected void PrintSubmittedActions()
    {
        string printMessage = "Total submitted actions: ";

        foreach (Action action in actionStack)
        {
            printMessage += action.DisplayName + ", ";
        }

        Debug.Log(printMessage);
    }

    public void TeamReady()
    {
        // Check to see if each team is ready
        bool allTeamsReady = true; // Let this be proven false
        foreach (TeamManager team in teams.Values)
        {
            if (!team.TeamReady) // This team is not ready
            {
                allTeamsReady = false;
                break;
            }
        }

        // If all teams are ready, go to the next phase
        if (allTeamsReady) // All teams are ready
        {
            StartCoroutine(NextPhase());
        }
    }

    protected IEnumerator NextPhase()
    {
        // Change current phase
        UpdateCurrentPhase();

        // Start the next phase
        Debug.Log(string.Format("<color=yellow>Phase Start: {0}</color>", currentPhase));

        // Update lists of actions for this phase
        UpdateActionLists();

        // Do different things depending on the phase
        switch (currentPhase)
        {
            case phase.PredictedDecision:
                yield return DecisionPhase(true);
                break;

            case phase.Decision:
                yield return DecisionPhase(false);
                break;

            case phase.Prep:
                yield return ActionPhase();
                break;
            case phase.Attack:
                yield return ActionPhase();
                break;
            case phase.Move:
                yield return ActionPhase();
                break;
        }

        // Update the UI
        uiManager.UpdateUI();
    }
    protected void UpdateCurrentPhase()
    {
        // Switch to the next phase
        switch (currentPhase)
        {
            case phase.PredictedDecision:
                currentPhase = phase.Decision;
                break;
            case phase.Decision:
                currentPhase = phase.Prep;
                break;
            case phase.Prep:
                currentPhase = phase.Attack;
                break;
            case phase.Attack:
                currentPhase = phase.Move;
                break;
            case phase.Move:
                currentPhase = phase.PredictedDecision;
                EndTurn();
                break;
        }
    }
    protected void UpdateActionLists()
    {
        actionsThisPhase.Clear();
        castsThisPhase.Clear();
        attacksThisPhase.Clear();
        buffsThisPhase.Clear();
        movesThisPhase.Clear();
        foreach (Action action in actionStack)
        {
            // Test if the action is in this phase
            if (action.Phase == currentPhase) // It is in this phase
            {
                // Save it as part of this phase
                actionsThisPhase.Add(action);

                // Test which type of action it is
                switch (action.ActionType)
                {
                    case actionType.cast: // Cast action
                        castsThisPhase.Add((CastAction)action);
                        break;

                    case actionType.attack: // Attack action
                        attacksThisPhase.Add((Attack)action);
                        break;

                    case actionType.aoeAttack: // AOE Attack action
                        attacksThisPhase.Add((AOEAttack)action);
                        break;

                    case actionType.move: // Move action
                        movesThisPhase.Add((Move)action);
                        break;

                    case actionType.buff: // Buff action
                        buffsThisPhase.Add((BuffAction)action);
                        break;
                }
            }
        }

    }

    protected void EndTurn()
    {
        // Remove creatures who died
        // TODO: Delete creatures to stop memory leaks
        foreach (Creature creature in creatures)
        {
            if (!creature.IsAlive)
            {
                deadCreatures.Add(creature);
            }
        }
        // Have to put it in a seperate loop because you can't change whats in a list while itterating through it
        foreach (Creature creature in deadCreatures)
        {
            creatures.Remove(creature);

            creature.Die();
        }
        deadCreatures.Clear();

        // Lower cooldowns and recharges and stuff
        foreach (TeamManager team in teams.Values)
        {
            team.EndTurn();
        }

        // Clear action list
        actionStack.Clear();
    }

    // PREDICTED DECISISION PHASE

    // DECISISION PHASE
    protected IEnumerator DecisionPhase(bool creaturesShouldBePredicted)
    {
        currentState = gameState.nothingSelected;
        // Check if everyone is being predicted (likely no), if everybody then move to the next phase. Else mark everyone who needs to decide on an action
        creaturesToDecideAction.Clear();
        foreach (Creature creature in creatures)
        {
            if (creature.Predicted == creaturesShouldBePredicted)
            { // This creature is being predicted and therefore this phase is nescesary
                creaturesToDecideAction.Add(creature);
            }
        }

        if (creaturesToDecideAction.Count == 0) // Everybody was being predicted so we can skip to the next phase
        {
            yield return NextPhase();
        }

        // Loop through all creatures yet to decide and ask them for an action
        foreach (Creature creature in creaturesToDecideAction)
        {
            creature.AI();
        }
    }

    // ACTION PHASE (a phase where actions are resolved, like Prep, Attack, and Move)
    protected IEnumerator ActionPhase()
    {
        CurrentState = gameState.uninteractable;

        // Update the UI
        uiManager.UpdateUI();

        // The lists actionsThisPhase, castsThisPhase, buffsThisPhase ect... are all updated already

        // Resolve cast actions
        yield return ResolveCastActions();

        // Resolve Attack actions
        yield return ResolveAttackActions();

        // Resolve Buff Actions
        yield return ResolveBuffActions();

        // Resolve Move Actions
        yield return ResolveMoveActions();

        // Go to the next phase
        yield break;
    }

    // Returns true if all cast actions are done
    protected IEnumerator ResolveCastActions()
    {
        //Debug.Log("ResolveCastActions() called");

        // Do each action, pausing in between
        foreach (CastAction cast in castsThisPhase)
        {
            // Move the camera to them
            cameraFocus.ShowPosition(cast.Source.Owner);

            // Wait for the camera to catch up
            yield return new WaitForSeconds(animationPauseTime);

            // Do the action
            cast.DoAction();

            // Start the timer
            yield return new WaitForSeconds(castAnimationTime);
        }
    }

    protected IEnumerator ResolveAttackActions()
    {
        //Debug.Log("ResolveAttackActions() called");

        // Recalculate all of the creatureTargets to see if they're still in range and line of sight (walls can be summoned
        // and creatures can dodge out of range of attacks)
        foreach (Attack attack in attacksThisPhase)
        {
            attack.CheckTargetsStillInRange();
            attack.RollToHit();
        }

        // Compile a list of potential targets and attackLines going to those targets
        possibleTargetsThisRound.Clear();
        // TODO: Actually delete the attack lines to not cause memory leaks
        attackLinesThisPhase.Clear();

        // Get a list of every creature that can be hit
        foreach (Creature creature in creatures)
        {
            // Add its location as a possible target
            possibleTargetsThisRound.Add(creature.Space);
        }

        // Also add attack origins that don't originate from the attacker
        foreach (Attack attack in attacksThisPhase)
        {
            if (!attack.OriginatesFromAttacker) // It does not originate from the attacker
            {
                // Add this to the list of potential targets
                possibleTargetsThisRound.Add(attack.Origin);
            }
        }

        // Loop through the list of attacks and create an attack line for each possible space it targets
        foreach (Attack attack in attacksThisPhase)
        {
            // Check each tile
            foreach (Tile target in attack.Targets)
            {
                if (possibleTargetsThisRound.Contains(target) && target != attack.Origin) // It is a valid target
                {
                    // Create a new attack line pointing there
                    attackLinesThisPhase.Add(new AttackLine(attack, target));
                }
                // Don't print that for every aoe target
                else if (attack.TargetType != targetTypes.aoe) // Its not a valid target and isn't AOE
                {
                    Debug.Log("Invalid targets for " + attack.DisplayName);
                }
            }

            // Add each missed attack
            foreach (Creature target in attack.MissedCreatureTargets)
            {
                // Create a new attack line, but include that it missed
                attackLinesThisPhase.Add(new AttackLine(attack, target.Space, true));
            }
        }

        // Check each attack line to join them together
        foreach (AttackLine attackLine in attackLinesThisPhase)
        {
            attackLine.Check(attackLinesThisPhase);
        }

        // TODO: Start on a pause and move the camera to show the animations during each pause

        // Start index at 0 (it gets increased by 1 before its checked)
        currentAttackLineIndex = -1;

        // Go to the first attack
        yield return NextAttack();
    }

    protected IEnumerator ResolveBuffActions()
    {
        //Debug.Log("ResolveBuffActions() called");

        // Do each action, pausing in between
        foreach (BuffAction buff in buffsThisPhase)
        {
            // Move the camera to them
            cameraFocus.ShowPosition(buff.Source.Owner);

            // Wait for the camera to catch up
            yield return new WaitForSeconds(animationPauseTime);

            // Do the action
            buff.DoAction();

            // Start the timer
            yield return new WaitForSeconds(buffAnimationTime);
        }
    }

    protected IEnumerator ResolveMoveActions()
    {
        //Debug.Log("ResolveMoveActions()");

        //longestMove = 0;
        foreach (Move move in movesThisPhase)
        {
            // Tell each moving creature that they're moving
            move.Source.Owner.MovingThisPhase = true;
        }

        currentMoveStep = 1;

        yield return MoveNextStep();
    }

    // ATTACK PHAES
/*    protected void AttackPhase()
    {
        currentState = gameState.uninteractable;
        // Recalculate all of the creatureTargets to see if they're still in range and line of sight (walls can be summoned
        // and creatures can dodge out of range of attacks)
        foreach (Action action in actionsThisPhase)
        {
            action.CheckTargetsStillInRange();
            action.RollToHit();
        }

        // Compile a list of potential targets and attackLines going to those targets
        possibleTargetsThisRound.Clear();
        // TODO: Actually delete the attack lines to not cause memory leaks
        attackLinesThisPhase.Clear();

        // Get a list of every creature that can be hit
        foreach (Creature creature in creatures)
        {
            // Add its location as a possible target
            possibleTargetsThisRound.Add(creature.Space);
        }

        // Also add attack origins that don't originate from the attacker
        foreach (Action action in actionsThisPhase)
        {
            if (!action.OriginatesFromAttacker) // It does not originate from the attacker
            {
                // Add this to the list of potential targets
                possibleTargetsThisRound.Add(action.Origin);
            }
        }

        // Loop through the list of attacks and create an attack line for each possible space it targets
        foreach (Action attack in actionsThisPhase)
        {
            // Check each tile
            foreach (Tile target in attack.Targets)
            {
                if (possibleTargetsThisRound.Contains(target) && target != attack.Origin) // It is a valid target
                {
                    // Create a new attack line pointing there
                    attackLinesThisPhase.Add(new AttackLine(attack, target));
                }
                // Don't print that for every aoe target
                else if (attack.TargetType != targetTypes.aoe) // Its not a valid target and isn't AOE
                {
                    Debug.Log("Invalid targets for " + attack.DisplayName);
                }
            }
        }

        // Check each attack line to join them together
        foreach (AttackLine attackLine in attackLinesThisPhase)
        {
            attackLine.Check(attackLinesThisPhase);
        }

        // TODO: Start on a pause and move the camera to show the animations during each pause

        // Start index at 0 (it gets increased by 1 before its checked)
        currentAttackLineIndex = -1;

        // Go to the first attack
        NextAttack();
    }
*/
    protected IEnumerator NextAttack()
    {
        // Go to the next index
        currentAttackLineIndex += 1;

        // Stop if there are no attacks left
        if (currentAttackLineIndex >= attackLinesThisPhase.Count) // There are no attacks left
        {
            // Run the cleanup code for each action (adding recharge and costing energy and stuff)
            foreach (Attack attack in attacksThisPhase)
            {
                attack.DoAction();
            }
            // Break out of NextAttack() (to finish ResolveAttackActions() and go to the next line in ActionPhase())
            yield break;
        }


        // Only execute the attackLine if its supposed to be executed
        if (attackLinesThisPhase[currentAttackLineIndex].ShouldBeExecuted) // this attack line should be executed
        {
            // Cache the attackLine for ease of reference
            AttackLine attackLine = attackLinesThisPhase[currentAttackLineIndex];

            // Point the camera at it
            attackLine.ShowInCamera(cameraFocus);

            // Wait for the camera to catch up
            yield return new WaitForSeconds(animationPauseTime);

            // Run this attack and start the timer
            attackLine.ResolveAttacks();
            yield return new WaitForSeconds(attackAnimationTime);
        }

        // Go to the next attack
        yield return NextAttack();
    }

    // MOVE PHASE
/*    protected void MovePhase()
    {
        currentState = gameState.uninteractable;
        Debug.Log("MovePhaseStart()");

        // Get the furthest move this phase
        longestMove = 0;
        foreach (Action move in actionsThisPhase)
        {
            if (move.TotalSteps > longestMove) // This move is longer than what was recorded to be the longest before
            {
                longestMove = move.TotalSteps;
            }

            // Also tell each moving creature that they're moving
            move.Source.Owner.MovingThisPhase = true;
        }

        currentMoveStep = 1;

        StartCoroutine(MoveNextStep());
    }*/
    protected IEnumerator MoveNextStep()
    {
        //Debug.Log("MoveNextStep() called");

        // Test if there are any moves left
        bool allMovesDone = true; // Remains true if all moves have finished (proven false if otherwise)
        foreach (Move move in movesThisPhase)
        {
            if (!move.MoveFinished) // This move is not finished
            {
                allMovesDone = false;
                break;
            }
        }

        if (allMovesDone) // There are no moves left
        {
            // Have each action cost their energy and stuff
            foreach (Move move in movesThisPhase)
            {
                // Have this move cost energy (and recharge and cooldown and all that)
                move.DoAction();
            }

            // Go to the next phase
            StartCoroutine(NextPhase());
            yield break;
        }


        // Tell each move action to go
        foreach (Move move in movesThisPhase)
        {
            move.PlayAnimation();
        }

        // Show all of the moves in the camera
        // TODO: This probably causes a memory leak
        List<Vector3> spaces = new List<Vector3> { };
        foreach (Move move in movesThisPhase)
        {
            spaces.Add(move.CurrentMoveToTile.RealPosition);
        }
        cameraFocus.ShowPosition(spaces, false);

        // Test if any moves would collide
        ResolveMoveCollisions();

        // Wait for the animations to play
        //yield return wait(moveTimer);
        yield return new WaitForSeconds(moveAnimationTime);

        // Finish each step
        foreach (Move move in movesThisPhase)
        {
            move.Source.Owner.FinishMove();
        }

        // Pause in between move steps
        //yield return wait(movePauseTimer);
        yield return new WaitForSeconds(moveAnimationPauseTime);

        // Recursively start the next step
        currentMoveStep += 1;
        yield return MoveNextStep();
    }


    // called every move step to check if creatures are colliding and resolve the result of that
    protected void ResolveMoveCollisions()
    {
        // Empty each creature's list of collisions this step
        foreach (Creature creature in creatures)
        {
            creature.CollisionsThisStep.Clear();
        }

        // Loop through each move this phase
        foreach (Move move in movesThisPhase)
        {
            // Test if the move is still happening
            if (!move.MoveFinished) // The move is still going
            {
                // Check with each creature if it will collide
                foreach (Creature creature in creatures)
                {
                    // Skip this creature if its the move's owner
                    if (creature == move.Source.Owner) // This move is looking at its own owner
                    {
                        // Skip this creature
                        continue;
                    }

                    // Test if this is 2 creatures moving to the same space, or a moving creature colliding with a stationary creature
                    // We already know move.Source.Owner is moving
                    if (creature.MovingThisPhase) // Both creatures are moving
                    {
                        // Test if they are going into the same spot
                        if (
                        creature.PreviousMoveToTile == move.CurrentMoveToTile && // This mover is going where the creature was
                        creature.TargetMoveToTile == move.PreviousMoveToTile) // This creature is going where the mover was
                        {
                            // Mark these 2 creatures as colliding with eachother
                            // (The other creature will also detect this collision and add it themselves)
                            move.Source.Owner.CollisionsThisStep.Add(creature);
                        }
                        else if (creature.TargetMoveToTile == move.CurrentMoveToTile) // Both creatures are moving to the same tile
                        {
                            // Mark these 2 creatures as colliding with eachother
                            // (The other creature will also detect this collision and add it themselves)
                            move.Source.Owner.CollisionsThisStep.Add(creature);
                        }
                    }
                    else // Only move.Source.Owner is moving
                    {
                        // Test if they will collide
                        if (move.CurrentMoveToTile == creature.Space) // Its going to ocupy the same space as another creature next move
                        {
                            // Mark these 2 creatures as colliding with eachother
                            move.Source.Owner.CollisionsThisStep.Add(creature);
                            creature.CollisionsThisStep.Add(move.Source.Owner);
                        }
                    }
                }
            }
        }

        // Resolve each collision
        foreach (Creature creature in creatures)
        {
            // Loop through each collision (while removing them from the list)
            // The collision we're testing will always be index 0
            for (int i = 0; i < creature.CollisionsThisStep.Count; i ++)
            {
                // Cache the other creature
                Creature otherCreature = creature.CollisionsThisStep[0];
                //Debug.Log("Collision resolution between " + creature.DisplayName + " and " + otherCreature.DisplayName + ". Other creature has " + otherCreature.CollisionsThisStep.Count + " collisions");

                // Make sure this collision hasn't already been resolved (both creatures have eachother in their lists)
                if (otherCreature.CollisionsThisStep.Contains(creature)) // The collision has not yet been resolved (both creatures have eachother in their lists)
                {
                    // Store the winner and loser of the strength check
                    Creature winner = StatTest(creature, creature.CollisionsThisStep[0], stats.strength);
                    Creature loser;
                    if (winner == creature) // The first creature won
                    {
                        loser = otherCreature;
                    }
                    else // The second creature won
                    {
                        loser = creature;
                    }

                    // Test who is moving
                    if (winner.MovingThisPhase && loser.MovingThisPhase) // Both creatures were moving 
                    {
                        // Attempt to push the loser if the winner is attempting to move into their current space
                        if (winner.TargetMoveToTile == loser.PreviousMoveToTile) // They were going to trade spaces (rather than both going for a single contested space)
                        {
                            // Attempt to push them
                            if (!loser.Push(winner.Space, 1)) // The push failed
                            {
                                winner.MovingThisPhase = false;
                            }
                        }

                        loser.MovingThisPhase = false;
                    }
                    else if (winner.MovingThisPhase) // Only the winner was moving (the loser was standing still)
                    {
                        Debug.Log("Only " + winner.DisplayName + " was moving, and they won");

                        // Attempt to push them
                        if (!loser.Push(winner.Space, 1)) // The push was not successful (they are in front of a wall or something)
                        {
                            winner.MovingThisPhase = false;
                        }
                    }
                    else // The loser was moving (the winner was standing still)
                    {
                        Debug.Log("Only " + loser.DisplayName + " was moving, and they won");

                        loser.MovingThisPhase = false;
                    }


                }

                // Remove this collision from the list
                creature.CollisionsThisStep.RemoveAt(0);
            }
        }
    }
    
    // Create or add to an existing team manager
    public void AddToTeam(Creature creature)
    {
        // Test if a new team manager needs to be created
        if (!teams.ContainsKey(creature.Team)) // The team manager doesn't yet exist
        {
            // Create the new team manager
            teams.Add(creature.Team, new TeamManager(creature.Team));
        }

        // Add the creature to the team manager
        teams[creature.Team].AddTeamMember(creature);
        // The creature records the team manager in AddTeamMember() in TeamManager.cs
    }

    public Creature StatTest(Creature creature1, Creature creature2, stats stat)
    {
        // Roll the dice
        int creature1Roll = UnityEngine.Random.Range(1, 21);
        int creature2Roll = UnityEngine.Random.Range(1, 21);

        // Assign the propper stat bonuses
        switch (stat)
        {
            case stats.strength:
                creature1Roll += creature1.Str;
                creature2Roll += creature2.Str;
                break;
            case stats.dexterity:
                creature1Roll += creature1.Dex;
                creature2Roll += creature2.Dex;
                break;
            case stats.intellect:
                creature1Roll += creature1.Int;
                creature2Roll += creature2.Int;
                break;
        }

        // Compare the rolls
        // TODO: Maybe have a window for contested rolls, same as clash attacks (or just use the clash attack window)
        if (creature1Roll > creature2Roll) // Creature 1 won
        {
            Debug.Log(stat + " test between " + creature1.DisplayName + " and " + creature2.DisplayName + ". " + creature1.DisplayName + " won!");
            return creature1;
        }
        else if (creature2Roll > creature1Roll) // Creature 2 won
        {
            Debug.Log(stat + " test between " + creature1.DisplayName + " and " + creature2.DisplayName + ". " + creature2.DisplayName + " won!");
            return creature2;
        }
        else // It was a tie
        {
            Debug.Log(stat + " test between " + creature1.DisplayName + " and " + creature2.DisplayName + ". Tie! Trying again...");
            return StatTest(creature1, creature2, stat);
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");

    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public Action ActiveAction(Creature creature)
    {
        // Look through all actions this phase and find the first one that is being done by this creature
        foreach (Action action in actionsThisPhase)
        {
            if (action.Source.Owner == creature)
            {
                return action;
            }
        }

        // Return null with an error if no action was found being done by this creature this phase
        Debug.LogError("ActiveAction() called for a creature not committing an action");
        return null;
    }
}