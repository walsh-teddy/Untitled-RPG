using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    PlayerManager playerManager;
    Pointer pointer;
    [SerializeField] CameraFocus cameraFocus;
    List<Creature> creatures = new List<Creature> { };
    List<Creature> creaturesToDecideAction = new List<Creature> { };
    static List<Action> actionStack = new List<Action> { };
    List<Action> actionsThisPhase = new List<Action> { };
    // Used in the attack phase to know which attacks are contested by what
    List<AttackLine> attackLinesThisRound = new List<AttackLine> { };
    List<Tile> possibleTargetsThisRound = new List<Tile> { };
    public const int CLASH_ATTACK_WINDOW = 1; // If 2 attacks roll within 1 of eachother, then they clash

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

    // METHODS
    private void Awake()
    {
        // Set up variables
        uiManager = gameObject.GetComponent<UIManager>();
        levelSpawner = gameObject.GetComponent<LevelSpawner>();
        playerManager = gameObject.GetComponent<PlayerManager>();
        pointer = gameObject.GetComponent<Pointer>();

        currentPhase = phase.predictedDescision;
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
        creatures.Add(newCreature);
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
                    // creature.SubmitAction();
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
        Debug.Log("MoveNextStep()");

        // Test if there are any moves left
        if (currentMoveStep > longestMove) // There are no moves left
        {
            NextPhase();
            return;
        }

        // TODO: Test if any moves would collide

        // Tell each move action to go
        foreach (Action move in actionsThisPhase)
        {
            move.DoAction();
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
        Debug.Log("FinishMove()");

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
        Debug.Log("FinishMovePause()");

        currentMoveStep += 1;

        MoveNextStep();
    }
    
}