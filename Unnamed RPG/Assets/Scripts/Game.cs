using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    // ATTRIBUTES
    public enum phase { predictedDescision, descision, prep, attack, move }
    static phase currentPhase;
    public enum weaponType { medium, heavy, light, shield, ranged, magic, none }
    public enum aoeType { circle, cone, line }
    public enum stats { strength, dexterity, intellect }
    public List<Weapon> thrownWeapons = new List<Weapon> { };
    UIManager uiManager;
    LevelSpawner levelSpawner;
    PlayerManager playerManager; // Included in teamMangers
    Dictionary<string, TeamManager> teams = new Dictionary<string, TeamManager> { };
    Pointer pointer;
    [SerializeField] CameraFocus cameraFocus;
    List<Creature> creatures = new List<Creature> { }; // Only contains the living creatures
    List<Creature> deadCreatures = new List<Creature> { };
    List<Creature> creaturesToDecideAction = new List<Creature> { };
    static List<Action> actionStack = new List<Action> { };
    List<Action> actionsThisPhase = new List<Action> { };
    // Used in the attack phase to know which attacks are contested by what
    List<AttackLine> attackLinesThisRound = new List<AttackLine> { };
    List<Tile> possibleTargetsThisRound = new List<Tile> { };
    [SerializeField] public const int CLASH_ATTACK_WINDOW = 1; // If 2 attacks roll within 1 of eachother, then they clash

    [Header("Animation")]
    [SerializeField] float moveAnimationTime = 1; // Time (in seconds) that a move animation takes
    [SerializeField] float moveAnimationPauseTime = 0.25f; // Time (in seconds) that the game pauses between each move
    [SerializeField] float attackAnimationTime = 2; // Time (in seconds) that a move animation takes
    [SerializeField] float attackAnimationPause = 0.25f; // Time (in seconds) that a move animation takes

    // MOVE PHASE LOGIC
    private float moveTimer;
    private float movePauseTimer;
    private bool moveTimerRunning = false;
    private bool movePauseTimerRunning = false;
    private int longestMove;
    private int currentMoveStep = 1;

    // States
    public enum gameState
    {
        uniteractable, // It is not in a decision phase and the player needs to wait for animatons to happen
        nothingSelected, // Nothing selected
        playerSelected, // Selected a player. Waiting for an action source to be selected
        playerSelectedSubmitted, // Selected a player who has already submitted an action
        playerActionSourceSelectAction, // Selected a player and an action source. Waiting to select an action
        playerActionSelectTarget, // Selecting the next tile for a player to move to
    }
    private gameState currentState = gameState.nothingSelected;

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

            // This should be irrelevant with the new UpdateUI()

/*            // Do stuff when changing away from the old state
            switch (currentState)
            {
                case gameState.playerSelectedSubmitted:
                    // Clear the player move widgets if they were shown
                    uiManager.HidePlayerMoveLines();
                    cameraFocus.LeavePerspective();
                    break;
                case gameState.playerActionSourceSelectAction:

                    break;
                case gameState.playerActionSelectTarget:
                    // Discard the move
                    playerManager.SelectedPlayer.DiscardAction();
                    uiManager.HidePlayerMoveLines();
                    cameraFocus.LeavePerspective();
                    break;
            }*/

            // Update the new state
            currentState = value;

            // Do stuff depending on the new state
            switch (currentState)
            {
                case gameState.playerSelected:
                    // Unhighlight all tiles
                    levelSpawner.UnHighlightAllTiles();
                    break;
                case gameState.playerSelectedSubmitted:
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

            // Update the UI
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
    public List<Weapon> ThrownWeapons
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
    private void Awake()
    {
        // Set up variables
        uiManager = gameObject.GetComponent<UIManager>();
        levelSpawner = gameObject.GetComponent<LevelSpawner>();
        pointer = gameObject.GetComponent<Pointer>();

        currentPhase = phase.predictedDescision;

        // Put the player manager in the dictionary of team managers
        playerManager = new PlayerManager();
        teams.Add("player", playerManager);
    }
    private void Start()
    {
        // Now that all the variables are set up (in Awake), push commands to other components
        levelSpawner.SpawnLevel();
        uiManager.UpdateUI();
    }
    private void Update()
    {
        // Move timer logic
        if (moveTimerRunning)
        {
            // Update the timer
            moveTimer += Time.deltaTime;

            // Update the player positions
            UpdateMove();

            // Check if its reached its goal
            if (moveTimer >= moveAnimationTime) // Its reached the end of the curent animation
            {
                moveTimerRunning = false;
                moveTimer = 0;
                FinishMove();
            }
        } 

        // Move pause timer
        else if (movePauseTimerRunning)
        {
            // Update the timer
            movePauseTimer += Time.deltaTime;

            // Check if its reached its goal
            if (movePauseTimer >= moveAnimationPauseTime) // Its reached the end of the current pause
            {
                movePauseTimerRunning = false;
                movePauseTimer = 0;
                FinishMovePause();
            }
        }
    }
    
    // A new creature was created and should be added to the list
    public void NewCreature(Creature newCreature)
    {
        // Record this creature in a list
        creatures.Add(newCreature);

        // Assign it a team manager
        AddToTeam(newCreature);
    }

    public void LevelComplete()
    {
        Debug.Log("Level Complete");
        foreach (Creature creature in creatures)
        {
            creature.UpdatePossibleTargets();
        }

        // Create the UI elements for each action source and action
        uiManager.CreateUI();

        // Start the first phase (predicted decision)
        PhaseStart();
    }

    // Most of the code for what happens in each phase (Prep, Attack, and Move are all automatic so every piece of
    // logic for them happens here)
    public void PhaseStart()
    {
        Debug.Log(string.Format("Phase Start: {0}", currentPhase));

        // Do different things depending on the phase
        switch (currentPhase)
        {
            case phase.predictedDescision:
                currentState = gameState.nothingSelected;

                // Check if anyone is being predicted, if not then move to the next phase. Else mark everyone who needs to decide on an action
                creaturesToDecideAction.Clear();
                foreach (Creature creature in creatures)
                {
                    if (creature.Predicted) { // This creature is being predicted and therefore this phase is nescesary
                        creaturesToDecideAction.Add(creature);
                    }
                }

                if (creaturesToDecideAction.Count == 0) // Nobody was being predicted so we can skip to the next phase
                {
                    NextPhase();
                    break;
                }

                // Loop through all creatures yet to decide and ask them for an action
                foreach (Creature creature in creaturesToDecideAction)
                {
                    creature.AI();
                }
                break;

            case phase.descision:
                // Check if everyone is being predicted (likely no), if everybody then move to the next phase. Else mark everyone who needs to decide on an action
                creaturesToDecideAction.Clear();
                foreach (Creature creature in creatures)
                {
                    if (!creature.Predicted)
                    { // This creature is being predicted and therefore this phase is nescesary
                        creaturesToDecideAction.Add(creature);
                    }
                }

                if (creaturesToDecideAction.Count == 0) // Everybody was being predicted so we can skip to the next phase
                {
                    NextPhase();
                    break;
                }

                // Loop through all creatures yet to decide and ask them for an action
                foreach (Creature creature in creaturesToDecideAction)
                {
                    creature.AI();
                }
                break;


            case phase.prep:
                // Turn off player controls (outside of camera controls)
                currentState = gameState.uniteractable;

                // Check if there are any actions in the prep phase, if no then move to the next phase. Else create a random order of which to do first
                actionsThisPhase.Clear();

                foreach (Action action in actionStack)
                {
                    if (action.Phase == phase.prep)
                    {
                        actionsThisPhase.Add(action);
                    }
                }

                while (actionsThisPhase.Count > 0)
                {
                    // Randomly select the next action in the queue to do
                    int randomIndex = Random.Range(0, actionsThisPhase.Count - 1);
                    actionsThisPhase[randomIndex].DoAction();
                    actionsThisPhase.RemoveAt(randomIndex);
                }

                NextPhase();

                break;
            case phase.attack:
                // Check if there are any actions in the attack phase, if no then move to the next phase. Else, create a random order of which to do first
                actionsThisPhase.Clear();

                foreach (Action action in actionStack)
                {
                    if (action.Phase == phase.attack)
                    {
                        actionsThisPhase.Add(action);
                    }
                }

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
                attackLinesThisRound.Clear();

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
                            attackLinesThisRound.Add(new AttackLine(attack, target));
                        }
                    }
                }

                // Check each attack line to join them together
                foreach (AttackLine attackLine in attackLinesThisRound)
                {
                    attackLine.Check(attackLinesThisRound);
                }

                // Resolve all attacks
                foreach (AttackLine attackLine in attackLinesThisRound)
                {
                    attackLine.ResolveAttacks();
                }

                // Do any more cleanup stuff
                foreach (Action action in actionsThisPhase)
                {
                    action.DoAction();
                }

                NextPhase();

                break;
            case phase.move:
                // Check if there are any actions in the move phase, if no then move to the next phase. Else, create a random order of which to do first.
                actionsThisPhase.Clear();

                foreach (Action action in actionStack)
                {
                    if (action.Phase == phase.move)
                    {
                        actionsThisPhase.Add(action);
                    }
                }

                MovePhaseStart();

                break;
        }

        // Update the UI
        uiManager.UpdateUI();
    }

    public void SubmitAction(Action action)
    {
        // Add the action to a list
        actionStack.Add(action);

        // Check if there is anyone left who still needs to submit an action (if not, go to the next phase)
        action.Source.Owner.HasSubmittedAction = true;
        bool allCreaturesDecided = true; // Let it be proven false
        foreach (Creature creature in creaturesToDecideAction)
        {
            if (!creature.HasSubmittedAction) // This creature has not yet submited an action
            {
                allCreaturesDecided = false;
            }
        }

        if (allCreaturesDecided) // This was the last creature to decide
        {
            // Move onto the next phase
            NextPhase();
        }
    }

    private void NextPhase()
    {
        // Switch to the next phase
        switch (currentPhase)
        {
            case phase.predictedDescision:
                currentPhase = phase.descision;
                break;
            case phase.descision:
                currentPhase = phase.prep;
                break;
            case phase.prep:
                currentPhase = phase.attack;
                break;
            case phase.attack:
                currentPhase = phase.move;
                break;
            case phase.move:
                currentPhase = phase.predictedDescision;
                NewRound();
                break;
        }

        

        // Start the next phase
        PhaseStart();
    }

    private void NewRound()
    {
        // TODO: Delete creatures who died
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

        // Lower cooldowns and recharges of remaining creatures
        foreach (Creature creature in creatures)
        {
            creature.EndTurn();
        }

        // Clear action list
        actionStack.Clear();
    }


    // MOVE PHASE LOGIC
    private void MovePhaseStart()
    {
        Debug.Log("MovePhaseStart()");
        // Get the furthest move this phase
        longestMove = 0;
        foreach (Action move in actionsThisPhase)
        {
            if (move.Targets.Count - 1 > longestMove) // This move is longer than what was recorded to be the longest before
            {
                longestMove = move.Targets.Count - 1;
            }
        }

        currentMoveStep = 1;

        MoveNextStep();
    }
    private void MoveNextStep()
    {
        // Test if there are any moves left
        if (currentMoveStep > longestMove) // There are no moves left
        {
            foreach (Action move in actionsThisPhase)
            {
                move.DoAction();
            }
            NextPhase();
            return;
        }

        // Test if any moves would collide
        foreach (Action move in actionsThisPhase)
        {
            // TODO: Make this work lol
            //move.CheckCollision(creatures);
        }

        // Tell each move action to go
        foreach (Action move in actionsThisPhase)
        {
            move.PlayAnimation();
        }

        // Start the timer
        moveTimerRunning = true;
    }
    // Called every frame creatures are moving
    private void UpdateMove()
    {
        float percentThere = moveTimer / moveAnimationTime;
        foreach (Action move in actionsThisPhase)
        {
            move.Source.Owner.DisplayMovePosition(percentThere);
        }
        // TODO: Call event or something
    }
    // Called once the move timer is done
    private void FinishMove()
    {
        // Tell each creature they reached the end of this move
        foreach (Action move in actionsThisPhase)
        {
            move.Source.Owner.FinishMove();
        }

        // Start the pause timer
        movePauseTimerRunning = true;
    }
    // Called once the pause between moves is done
    private void FinishMovePause()
    {
        currentMoveStep += 1;

        MoveNextStep();
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

    public Creature StatTest(Creature creature1, Creature creature2, Game.stats stat)
    {
        // Roll the dice
        int creature1Roll = Random.Range(1, 21);
        int creature2Roll = Random.Range(1, 21);

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
            Debug.Log(stat + " test between " + creature1.DisplayName + " and " + creature2.DisplayName + ". Tie!");
            // TODO: Find a better way to resolve this lol
            return null;
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
}