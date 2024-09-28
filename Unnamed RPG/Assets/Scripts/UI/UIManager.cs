using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // TODO: Can probably get rid of displayText soon
    [SerializeField] protected TextMeshProUGUI displayText;
    [SerializeField] protected Canvas canvas;

    [Header("Colors")]
    // TODO: Make this more readable and sturdy if the order of the phases change
    [SerializeField] protected List<Color> phaseColors; // A list of colors that each phase uses (goes in order -> PredictedDecision, Decision, Prep, Attack, Move

    // Action Bar
    [Header("Action Bar")]
    [SerializeField] protected GameObject actionBar;
    [SerializeField] protected GameObject backButton;
    [SerializeField] protected GameObject submitButton;
    [SerializeField] protected GameObject uiRootOrigin;
    [SerializeField] protected GameObject uiRootPrefab;
    [SerializeField] protected GameObject uiRootCenterOrigin;
    [SerializeField] protected GameObject uiRootCenterPrefab;
    [SerializeField] protected GameObject actionSourceButtonPrefab;
    [SerializeField] protected GameObject actionButtonPrefab;
    [SerializeField] protected GameObject undoButtonPrefab;
    [SerializeField] protected GameObject undoButtonRootPrefab;

    [Header("Special Action Buttons")]
    [SerializeField] protected GameObject uiMoveChaseButtonPrefab;

    [Header("Phase Bar")]
    [SerializeField] protected List<uiPhaseBar> phaseBarPannels;
    [SerializeField] protected float activeColorAlpha = 1;
    [SerializeField] protected float innactiveColorAlpha = 0.2f;
    public float ActiveColorAlpha
    {
        get { return activeColorAlpha; }
    }
    public float InnactiveColorAlpha
    {
        get { return innactiveColorAlpha; }
    }

    // Player Move Line Renderer
    [Header("Widgets")]
    [SerializeField] protected GameObject tilePointPrefab;
    [SerializeField] protected GameObject lineRendererPrefab;
    [SerializeField] protected GameObject selectedCreatureHighlight;
    [SerializeField] protected GameObject targetHighlightPrefab;
    protected List<MeshRenderer> tilePointList = new List<MeshRenderer> { };
    protected List<LineRenderer> lineRendererList = new List<LineRenderer> { };
    protected List<GameObject> targetHighlights = new List<GameObject> { };
    [SerializeField] protected float tilePointOffset = 1.0f; // How far above a tile the tile point hovers
    [SerializeField] protected int startingWidgitCount = 5;
    [SerializeField] protected GameObject plannedMovementMarkerPrefab;
    protected List<GameObject> plannedMovementMarkerList = new List<GameObject> { };

    [Header("Other")]
    [SerializeField] protected GameObject loadingScreen;
    [SerializeField] protected GameObject readyButton;

    // Other scripts
    protected LevelSpawner levelSpawner;
    protected PlayerManager playerManager;
    protected Game game;


    // Properties
    public GameObject ActionBar
    {
        get { return actionBar; }
    }
    public TextMeshProUGUI DisplayText
    {
        get { return displayText; }
    }

    // Prefabs
    public GameObject UIRootPrefab
    {
        get { return uiRootPrefab; }
    }
    public GameObject UIRootCenterPrefab
    {
        get { return uiRootCenterPrefab; }
    }
    public Transform UIRootCenterOrigin
    {
        get { return uiRootCenterOrigin.transform; }
    }
    public GameObject ActionSourceButtonPrefab
    {
        get { return actionSourceButtonPrefab; }
    }
    public GameObject ActionButtonPrefab
    {
        get { return actionButtonPrefab; }
    }
    public GameObject UndoButtonPrefab
    {
        get { return undoButtonRootPrefab; }
    }
    public GameObject UIMoveChaseButtonPrefab
    {
        get { return uiMoveChaseButtonPrefab; }
    }

    protected void Awake()
    {
        levelSpawner = gameObject.GetComponent<LevelSpawner>();
        game = gameObject.GetComponent<Game>();
    }

    protected void Start()
    {
        // Start each list at startingWidgitCount
        CreateTilePoints(startingWidgitCount);
        CreateLineRenderers(startingWidgitCount);
        CreateTargetHighlights(startingWidgitCount);
        CreatePlannedMovementMarkers(startingWidgitCount);
    }

    private void CreateTilePoints(int totalTilePoints)
    {
        // Create new tile points until it matches the total
        while (tilePointList.Count < totalTilePoints)
        {
            tilePointList.Add(Instantiate(tilePointPrefab).GetComponent<MeshRenderer>());
        }
    }
    private void CreateLineRenderers(int totalLineRenderers)
    {
        // Create new line renderers until it matches the total
        while (lineRendererList.Count < totalLineRenderers)
        {
            lineRendererList.Add(Instantiate(lineRendererPrefab).GetComponent<LineRenderer>());
        }
    }
    private void CreateTargetHighlights(int totalTargetHighlights)
    {
        // Create new target highlights until it matches the total
        while (targetHighlights.Count < totalTargetHighlights)
        {
            targetHighlights.Add(Instantiate(targetHighlightPrefab));
        }
    }
    private void CreatePlannedMovementMarkers(int totalPlannedMovementMarkers)
    {
        // Create new planned movement markers until it matches the total
        while (plannedMovementMarkerList.Count < totalPlannedMovementMarkers)
        {
            plannedMovementMarkerList.Add(Instantiate(plannedMovementMarkerPrefab));
        }
    }

    // After the map is created, create all the UI elements for each player
    public virtual void CreateUI()
    {
        Debug.Log("Creating UI");

        // Cache the player manager
        playerManager = game.PlayerManager;

        // Loop through each player
        foreach (Player player in playerManager.Players)
        {
            // Create the UI for that player
            // Each player will also create their own ActionSource UI and Action UI
            player.CreateUI(uiRootOrigin.transform);
        }

        UpdateUI();
    }

    // Methods
    public virtual void UpdateUI()
    {
        // Reset everything
        HidePlayerMoveLines();
        HideTargetHighlights();
        selectedCreatureHighlight.SetActive(false);
        levelSpawner.UnHighlightAllTiles();

        // Create variables to cache data into for ease of reference
        Player player;
        Action action;

        // Update the action bar based on the current state of what is selected
        switch (game.CurrentState)
        {
            // Nothing is selected. Display nothing until a player is selected
            case gameState.nothingSelected:

                // Update the display text
                displayText.text = "";

                // Turn the action pannel off
                actionBar.SetActive(false);

                // No tiles should be highlighted
                levelSpawner.UnHighlightAllTiles();

                // Turn on the ready button
                readyButton.SetActive(true);

                break;

            // A player is selected. Prompt them to select an action
            case gameState.playerSelected:

                // Cache variables for ease of reference
                player = playerManager.SelectedPlayer;

                // Update the display text
                displayText.text = string.Format("{0}\nSelect an action", player.DisplayName);

                // Turn the action pannel on
                actionBar.SetActive(true);

                // Update the action pannel buttons
                player.UIRoot.SetActive(true);
                player.SourceButtonRoot.SetActive(true);
                backButton.SetActive(true);
                submitButton.SetActive(false);

                // Highlight the selected creature but no target
                selectedCreatureHighlight.transform.position = player.Space.RealPosition;
                selectedCreatureHighlight.SetActive(true);

                // No tiles should be highlighted
                levelSpawner.UnHighlightAllTiles();

                // Turn on the ready button
                readyButton.SetActive(true);

                break;

            // A player and one of their action sources are selected and they need to pick an action
            case gameState.playerActionSourceSelectAction:

                // Cache variables for ease of reference
                player = playerManager.SelectedPlayer;

                // Update the display text
                displayText.text = string.Format("{0}\n{1} selected. Select an action",
                    player.DisplayName,
                    player.SelectedActionSource.DisplayName
                );

                // TODO: Turn on specific action source buttons
                // The player is chosing a target for their action
                actionBar.SetActive(true);
                player.UIRoot.SetActive(true);
                player.SourceButtonRoot.SetActive(false);
                player.SelectedActionSource.UIRoot.SetActive(true);
                backButton.SetActive(true);
                submitButton.SetActive(false);

                // Highlight the selected creature
                selectedCreatureHighlight.transform.position = player.Space.RealPosition;
                selectedCreatureHighlight.SetActive(true);

                // Turn off the ready button
                readyButton.SetActive(false);

                break;

            // A player and one of their actions are selected and they need to pick a target for the action
            case gameState.playerActionSelectTarget:

                player = playerManager.SelectedPlayer;
                action = player.SelectedAction;

                // Update the display text
                displayText.text = string.Format("{0}\n{1} selected from {2}\nSelect a target",
                    player.DisplayName,
                    player.SelectedAction.DisplayName,
                    player.SelectedActionSource.DisplayName
                );

                // Turn the action pannel on (its probably already on but this is a failsafe
                actionBar.SetActive(true);

                // The player is chosing a target for their action
                player.UIRoot.SetActive(true);
                player.SourceButtonRoot.SetActive(false);
                player.SelectedActionSource.UIRoot.SetActive(false);
                player.SelectedAction.UIRoot.SetActive(true);
                backButton.SetActive(true);
                submitButton.SetActive(true);

                // Highlight the selected creature and no target (might be redundant but safe)
                selectedCreatureHighlight.transform.position = player.Space.RealPosition;
                selectedCreatureHighlight.SetActive(true);

                // Update the player move line widgets
                // Highlight the creature targeted with an attack if there is one
                switch (action.TargetType)
                {
                    case targetTypes.single:

                        // Highlight all targets
                        UpdateTargetHighlights(action.Targets);
                        break;

                    case targetTypes.aoe: // AOE Attack
                        // Highlight all creatures in target area
                        UpdateTargetHighlights(action.AOETilesWithCreatures);
                        break;

                    case targetTypes.move: // Move
                        // Display player move lines
                        UpdatePlayerMoveLines(action.Targets);
                        break;
                }

                // Highlight the attack range and potential targets
                levelSpawner.HighlightTiles(
                    action.PossibleSpaces, // Light highlight
                    action.PossibleTargets // Medium highlight
                );

                // Turn off the ready button
                readyButton.SetActive(false);

                break;

            case gameState.uninteractable:

                // Turn off the action pannel
                actionBar.SetActive(false);

                // Turn off the ready button
                readyButton.SetActive(false);
                break;

        }

        // UPDATE THE PHASE BAR
        foreach (uiPhaseBar pannel in phaseBarPannels)
        {
            pannel.UpdateUI();
        }

        // Update all creature UI
        foreach (Creature creature in game.Creatures)
        {
            creature.UpdateUI();
        }

        // Remove the loading screen
        loadingScreen.SetActive(false);
    }

    // Updates the line renderer to point to the different tiles the player is moving to
    public void UpdatePlayerMoveLines(List<Tile> tileList)
    {

        // If the tileList is 3 tiles long, there should be 3 tile points and 2 line renderers
        // Tile point count = tile list count
        // Line renderer count = tile list count - 1

        // Loop through and disable every widget
        HidePlayerMoveLines();

        CreateTilePoints(tileList.Count);
        // Go through each tile and add a tile point to it
        for (int i = 0; i < tileList.Count; i++)
        {
            // Put a tile point over the tile
            Vector3 targetPosition = tileList[i].RealPosition;
            targetPosition.y += tilePointOffset;
            tilePointList[i].gameObject.transform.position = targetPosition;

            // enable the mesh renderer
            tilePointList[i].enabled = true;
        }

        // Go through each tile point, starting with the 2nd, and connect a line renderer to it
        // and the previous point
        // "This" line renderer is the previous index

        // Make sure there are enough line renderers
        CreateLineRenderers(tileList.Count - 1);

        for (int i = 1; i < tileList.Count; i++)
        {
            // Attatch the line renderer to this and the last tile points
            lineRendererList[i - 1].SetPosition(0, tilePointList[i].gameObject.transform.position);
            lineRendererList[i - 1].SetPosition(1, tilePointList[i - 1].gameObject.transform.position);

            // Enable the line renderer
            lineRendererList[i - 1].enabled = true;
        }

        // ALSO Update the planned movement markers
        // Get the planned movement of all allies
        List<Tile> plannedMovementThisPhase = playerManager.PlannedMovementAtStep(tileList.Count, playerManager.SelectedPlayer.SelectedAction.Phase, playerManager.SelectedPlayer);

        // Make sure there are enough planned movement markers
        CreatePlannedMovementMarkers(plannedMovementThisPhase.Count);

        // Loop through that list and put a marker down on each tile
        for (int i = 0; i < plannedMovementThisPhase.Count; i++)
        {
            plannedMovementMarkerList[i].transform.position = plannedMovementThisPhase[i].RealPosition;
            plannedMovementMarkerList[i].SetActive(true);
        }
    }
    public void HidePlayerMoveLines()
    {
        // Turn off all tile points
        foreach (MeshRenderer tilePoint in tilePointList)
        {
            tilePoint.enabled = false;
        }

        // Turn off all line renderers
        foreach (LineRenderer lineRenderer in lineRendererList)
        {
            lineRenderer.enabled = false;
        }

        // Turn off all planned movement markers
        foreach (GameObject plannedMovementMarker in plannedMovementMarkerList)
        {
            plannedMovementMarker.SetActive(false);
        }
    }

    public void UpdateTargetHighlights(List<Tile> targets)
    {
        // TODO: Make each creature instead have a highlight childed to them thats turned on or off, rather than having multiple highlights that are moved around



        HideTargetHighlights();

        // Make sure there are enough target highlights
        CreateTargetHighlights(targets.Count);

        // put a highlight on each given target
        for (int i = 0; i < targets.Count; i ++)
        {
            // Put this highlight on this target
            targetHighlights[i].transform.position = targets[i].RealPosition;
            targetHighlights[i].SetActive(true);
        }
    }

    public void HideTargetHighlights()
    {
        // Loop through and disable every widget
        foreach (GameObject targetHighlight in targetHighlights)
        {
            targetHighlight.SetActive(false);
        }
    }

    // TODO: Move the logic from the player manager to here
    public void SubmitActionButtonClicked()
    {
        playerManager.SubmitActionButtonClicked();
    }
    public void BackButtonClicked()
    {
        playerManager.BackButtonClicked();
    }
    public void ReadyButtonClicked()
    {
        // TODO: Prompt the player if they're readying up with some characters submitting no actions
        playerManager.ReadyUp();
    }

    // Called by Player.cs
    public GameObject CreateUndoButton(Action submittedAction, GameObject uiRoot)
    {
        GameObject button = Instantiate(undoButtonPrefab, uiRoot.transform);
        //button.transform.Translate(undoButtonXOffset, verticalButtonSpacing * index, 0f);
        button.GetComponent<uiUndoButton>().Create(submittedAction);
        return button;
    }

    // Return the color assosiated with that phase
    // TODO: This would work better as a dictionary, but those are not serializable
    public Color ColorByPhase(phase phase)
    {
        switch (phase)
        {
            case phase.PredictedDecision:
                return phaseColors[0];
            case phase.Decision:
                return phaseColors[1];
            case phase.Prep:
                return phaseColors[2];
            case phase.Attack:
                return phaseColors[3];
            case phase.Move:
                return phaseColors[4];
        }

        // Backup incase for some reason the switch statemnt doesn't work
        return phaseColors[0];
    }
}
