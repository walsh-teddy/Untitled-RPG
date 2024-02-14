using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI displayText;
    [SerializeField] Canvas canvas;

    // Action Bar
    [Header("Action Bar")]
    [SerializeField] GameObject actionPannel;
    [SerializeField] GameObject backButton;
    [SerializeField] GameObject submitButton;
    [SerializeField] GameObject undoButton;
    [SerializeField] GameObject buttonOriginLocation;
    [SerializeField] GameObject testTextPrefab;
    [SerializeField] GameObject uiRootPrefab;
    [SerializeField] GameObject actionSourceButtonPrefab;
    [SerializeField] GameObject actionButtonPrefab;
    [SerializeField] float buttonSpacing = 10f;

    // Phase Bar
    [Header("Phase Bar")]
    [SerializeField] GameObject PredictedDescisionBarFaded;
    [SerializeField] GameObject PredictedDescisionBarFull;
    [SerializeField] GameObject decisionBarFaded;
    [SerializeField] GameObject decisionBarFull;
    [SerializeField] GameObject prepBarFaded;
    [SerializeField] GameObject prepBarFull;
    [SerializeField] GameObject attackBarFaded;
    [SerializeField] GameObject attackBarFull;
    [SerializeField] GameObject movementBarFaded;
    [SerializeField] GameObject movementBarFull;
    Game.phase oldPhase = Game.phase.predictedDescision;

    // Player Move Line Renderer
    [Header("Widgets")]
    [SerializeField] GameObject tilePointPrefab;
    [SerializeField] GameObject lineRendererPrefab;
    [SerializeField] GameObject selectedCreatureHighlight;
    [SerializeField] GameObject targetHighlightPrefab;
    List<MeshRenderer> tilePointList = new List<MeshRenderer> { };
    List<LineRenderer> lineRendererList = new List<LineRenderer> { };
    List<GameObject> targetHighlights = new List<GameObject> { };
    [SerializeField] float tilePointOffset = 1.0f; // How far above a tile the tile point hovers
    [SerializeField] int widgetCount = 5;

    // Other scripts
    LevelSpawner levelSpawner;
    PlayerManager playerManager;
    Game game;
    

    // Properties
    public GameObject ActionPannel
    {
        get { return actionPannel; }
    }
    public TextMeshProUGUI DisplayText
    {
        get { return displayText; }
    }

    private void Awake()
    {
        levelSpawner = gameObject.GetComponent<LevelSpawner>();
        playerManager = gameObject.GetComponent<PlayerManager>();
        game = gameObject.GetComponent<Game>();
    }

    private void Start()
    {
        // Create a list of line renderers and tile points for player move lines
        for (int i = 0; i < widgetCount; i++)
        {
            // Create a new tile point
            tilePointList.Add(Instantiate(tilePointPrefab).GetComponent<MeshRenderer>());

            // Create a new line renderer
            lineRendererList.Add(Instantiate(lineRendererPrefab).GetComponent<LineRenderer>());

            // Create a new selected target highlight
            targetHighlights.Add(Instantiate(targetHighlightPrefab));
        }
    }

    // After the map is created, create all the UI elements for each player
    public void CreateUI()
    {
        Debug.Log("Creating UI");
        // Loop through each player
        foreach (Player player in playerManager.Players)
        {
            // Create a root element for the player onto the canvas
            player.UIRoot = Instantiate(uiRootPrefab, buttonOriginLocation.transform);
            player.UIRoot.SetActive(false);
            player.UIRoot.name = player.DisplayName + "UIRoot";

            // Create a root for the action source buttons to use when turning on or off
            player.SourceButtonRoot = Instantiate(uiRootPrefab, player.UIRoot.transform);
            player.SourceButtonRoot.SetActive(false);
            player.SourceButtonRoot.name = player.DisplayName + "SourceButtonRoot";

            // Loop through each action source
            float moveSourceToRight = 0; // How far to the right the button should move
            foreach (ActionSource actionSource in player.ActionSources)
            {
                // Create a root element childed to the player ui root
                actionSource.UIRoot = Instantiate(uiRootPrefab, player.UIRoot.transform);
                actionSource.UIRoot.SetActive(false);
                actionSource.UIRoot.name = actionSource.DisplayName + "UIRoot"; // TODO: Remove this (this is for debugging)

                // Create a button for the action source (childed to the player source button root)
                // TODO: Make a button prefab
                actionSource.UIButton = Instantiate(actionSourceButtonPrefab, player.SourceButtonRoot.transform);
                actionSource.UIButton.name = actionSource.DisplayName + "UIButton"; // TODO: Remove this (this is for debugging)

                // Translate the button to the right
                actionSource.UIButton.transform.Translate(moveSourceToRight, 0, 0);
                // Itterate moveSourceToRight
                moveSourceToRight += buttonSpacing;

                // Change the text of the button
                actionSource.UIButton.GetComponentInChildren<TextMeshProUGUI>().text = actionSource.DisplayName;

                // Loop through each action in the action source
                float moveActionToRight = 0; // How far to the right the button should move
                foreach (Action action in actionSource.ActionList)
                {
                    // Create a button for the action (childed to the action source root)
                    // TODO: Make a button prefab
                    action.UIButton = Instantiate(actionButtonPrefab, actionSource.UIRoot.transform);
                    action.UIButton.name = action.DisplayName + "UIButton"; // TODO: Remove this (this is for debugging)

                    // Translate the button to the right
                    action.UIButton.transform.Translate(moveActionToRight, 0, 0);
                    // Itterate moveSourceToRight
                    moveActionToRight += buttonSpacing;

                    // Change the text of the button
                    action.UIButton.GetComponentInChildren<TextMeshProUGUI>().text = action.DisplayName;
                }
            }
        }
    }

    // Methods
    public void UpdateUI()
    {
        // Reset everything
        HidePlayerMoveLines();
        HideTargetHighlights();
        selectedCreatureHighlight.SetActive(false);
        levelSpawner.UnHighlightAllTiles();

        // Update the action bar based on the current state of what is selected
        switch (game.CurrentState)
        {
            // Nothing is selected. Display nothing until a player is selected
            case Game.gameState.nothingSelected:

                // Update the display text
                displayText.text = "";

                // Turn the action pannel off
                actionPannel.SetActive(false);

                // No tiles should be highlighted
                levelSpawner.UnHighlightAllTiles();

                break;

            // A player is selected. Prompt them to select an action
            case Game.gameState.playerSelected:

                // Update the display text
                displayText.text = string.Format("{0}\nSelect an action", playerManager.SelectedPlayer.DisplayName);

                // Turn the action pannel on
                actionPannel.SetActive(true);

                // Update the action pannel buttons
                playerManager.SelectedPlayer.UIRoot.SetActive(true);
                playerManager.SelectedPlayer.SourceButtonRoot.SetActive(true);
                backButton.SetActive(true);
                submitButton.SetActive(false);
                undoButton.SetActive(false);

                // Highlight the selected creature but no target
                selectedCreatureHighlight.transform.position = playerManager.SelectedPlayer.Space.realPosition;
                selectedCreatureHighlight.SetActive(true);

                // No tiles should be highlighted
                levelSpawner.UnHighlightAllTiles();

                break;

            case Game.gameState.playerSelectedSubmitted:

                // Update the display text
                displayText.text = string.Format("{0}\nAlready submitted action", playerManager.SelectedPlayer.DisplayName);

                // Update the action pannel buttons
                actionPannel.SetActive(true);
                playerManager.SelectedPlayer.UIRoot.SetActive(true);
                playerManager.SelectedPlayer.SourceButtonRoot.SetActive(true);
                backButton.SetActive(true);
                submitButton.SetActive(false);
                undoButton.SetActive(true);

                // Highlight the selected creature
                selectedCreatureHighlight.transform.position = playerManager.SelectedPlayer.Space.realPosition;
                selectedCreatureHighlight.SetActive(true);

                // Highlight the creature targeted with an attack if there is one
                if (playerManager.SelectedPlayer.SelectedAction.IsAttack) // They have chosen an attack with 1 target
                {
                    UpdateTargetHighlights(playerManager.SelectedPlayer.SelectedAction.Targets);
                } 
                else if (playerManager.SelectedPlayer.SelectedAction.IsMove) // They have chosen a move
                {
                    UpdatePlayerMoveLines(playerManager.SelectedPlayer.SelectedAction.Targets);
                }

                break;

            // A player and one of their action sources are selected and they need to pick an action
            case Game.gameState.playerActionSourceSelectAction:

                // Update the display text
                displayText.text = string.Format("{0}\n{1} selected. Select an action",
                    playerManager.SelectedPlayer.DisplayName,
                    playerManager.SelectedPlayer.SelectedActionSource.DisplayName
                );

                // TODO: Turn on specific action source buttons
                // The player is chosing a target for their action
                actionPannel.SetActive(true);
                playerManager.SelectedPlayer.UIRoot.SetActive(true);
                playerManager.SelectedPlayer.SourceButtonRoot.SetActive(false);
                playerManager.SelectedPlayer.SelectedActionSource.UIRoot.SetActive(true);
                backButton.SetActive(true);
                submitButton.SetActive(true);
                undoButton.SetActive(false);

                // Highlight the selected creature
                selectedCreatureHighlight.transform.position = playerManager.SelectedPlayer.Space.realPosition;
                selectedCreatureHighlight.SetActive(true);

                break;

            // A player and one of their actions are selected and they need to pick a target for the action
            case Game.gameState.playerActionSelectTarget:

                // Update the display text
                // TODO: Update the display text with stuff more specific to the action
                displayText.text = string.Format("{0}\n{1} selected from {2}.\nSelect a target",
                    playerManager.SelectedPlayer.DisplayName,
                    playerManager.SelectedPlayer.SelectedAction.DisplayName,
                    playerManager.SelectedPlayer.SelectedActionSource.DisplayName
                );

                // Turn the action pannel on (its probably already on but this is a failsafe
                actionPannel.SetActive(true);

                // The player is chosing a target for their action
                playerManager.SelectedPlayer.UIRoot.SetActive(true);
                playerManager.SelectedPlayer.SourceButtonRoot.SetActive(false);
                playerManager.SelectedPlayer.SelectedActionSource.UIRoot.SetActive(false);
                backButton.SetActive(true);
                submitButton.SetActive(true);
                undoButton.SetActive(false);

                // Highlight the selected creature and no target (might be redundant but safe)
                selectedCreatureHighlight.transform.position = playerManager.SelectedPlayer.Space.realPosition;
                selectedCreatureHighlight.SetActive(true);

                // Update the player move line widgets
                // Highlight the creature targeted with an attack if there is one
                if (playerManager.SelectedPlayer.SelectedAction.IsAttack && !playerManager.SelectedPlayer.SelectedAction.IsAOE) // They have chosen a non-AOE attack
                {
                    UpdateTargetHighlights(playerManager.SelectedPlayer.SelectedAction.Targets);

                }
                else if (playerManager.SelectedPlayer.SelectedAction.IsAttack && playerManager.SelectedPlayer.SelectedAction.IsAOE) // They have chosen an AOE attack
                {
                    UpdateTargetHighlights(playerManager.SelectedPlayer.SelectedAction.AOETilesWithCreatures);

                }
                else if (playerManager.SelectedPlayer.SelectedAction.IsMove) // They have chosen a move
                {
                    UpdatePlayerMoveLines(playerManager.SelectedPlayer.SelectedAction.Targets);
                }

                // Highlight the attack range and potential targets
                levelSpawner.HighlightTiles(
                    playerManager.SelectedPlayer.SelectedAction.PossibleSpaces, // Light highlight
                    playerManager.SelectedPlayer.SelectedAction.PossibleTargets // Medium highlight
                );

                break;
        }

        // UPDATE THE PHASE BAR

        // Unhighlight the old phase
        switch (oldPhase)
        {
            case Game.phase.predictedDescision:
                PredictedDescisionBarFaded.SetActive(true);
                PredictedDescisionBarFull.SetActive(false);
                break;
            case Game.phase.descision:
                decisionBarFaded.SetActive(true);
                decisionBarFull.SetActive(false);
                break;
            case Game.phase.prep:
                prepBarFaded.SetActive(true);
                prepBarFull.SetActive(false);
                break;
            case Game.phase.attack:
                attackBarFaded.SetActive(true);
                attackBarFull.SetActive(false);
                break;
            case Game.phase.move:
                movementBarFaded.SetActive(true);
                movementBarFull.SetActive(false);
                break;
        }

        // Switch to and highlight the new phase
        oldPhase = game.CurrentPhase;
        switch (game.CurrentPhase)
        {
            case Game.phase.predictedDescision:
                PredictedDescisionBarFaded.SetActive(false);
                PredictedDescisionBarFull.SetActive(true);
                break;
            case Game.phase.descision:
                decisionBarFaded.SetActive(false);
                decisionBarFull.SetActive(true);
                break;
            case Game.phase.prep:
                prepBarFaded.SetActive(false);
                prepBarFull.SetActive(true);
                break;
            case Game.phase.attack:
                attackBarFaded.SetActive(false);
                attackBarFull.SetActive(true);
                break;
            case Game.phase.move:
                movementBarFaded.SetActive(false);
                movementBarFull.SetActive(true);
                break;
        }
    }

    // Updates the line renderer to point to the different tiles the player is moving to
    public void UpdatePlayerMoveLines(List<Tile> tileList)
    {
        // Don't do anything if the widgets don't exist yet
        if (tilePointList.Count < 1 || lineRendererList.Count < 1)
        {
            // Break out of the function
            return;
        }

        // If the tileList is 3 tiles long, there should be 3 tile points and 2 line renderers
        // Tile point count = tile list count
        // Line renderer count = tile list count - 1

        // Loop through and disable every widget
        for (int i = 0; i < widgetCount; i++)
        {
            tilePointList[i].enabled = false;
            lineRendererList[i].enabled = false;
        }

        // Go through each tile and add a tile point to it
        for (int i = 0; i < tileList.Count; i++)
        {
            // Put a tile point over the tile
            Vector3 targetPosition = tileList[i].realPosition;
            targetPosition.y += tilePointOffset;
            tilePointList[i].gameObject.transform.position = targetPosition;

            // enable the mesh renderer
            tilePointList[i].enabled = true;
        }

        // Go through each tile point, starting with the 2nd, and connect a line renderer to it
        // and the previous point
        // "This" line renderer is the previous index
        for (int i = 1; i < tileList.Count; i++)
        {
            // Attatch the line renderer to this and the last tile points
            lineRendererList[i - 1].SetPosition(0, tilePointList[i].gameObject.transform.position);
            lineRendererList[i - 1].SetPosition(1, tilePointList[i - 1].gameObject.transform.position);

            // Enable the line renderer
            lineRendererList[i - 1].enabled = true;
        }
    }
    public void HidePlayerMoveLines()
    {
        // Don't do anything if the widgets don't exist yet
        if (tilePointList.Count < 1 || lineRendererList.Count < 1)
        {
            // Break out of the function
            return;
        }

        // Loop through and disable every widget
        for (int i = 0; i < widgetCount; i++)
        {
            tilePointList[i].enabled = false;
            lineRendererList[i].enabled = false;
        }
    }

    public void UpdateTargetHighlights(List<Tile> targets)
    {
        // Don't do anything if the widgets don't exist yet
        if (targetHighlights.Count < 1)
        {
            // Break out of the function
            return;
        }

        // Loop through and disable every widget
        foreach (GameObject targetHighlight in targetHighlights)
        {
            targetHighlight.SetActive(false);
        }

        // put a highlight on each given target
        for (int i = 0; i < targets.Count; i ++)
        {
            // Put this highlight on this target
            targetHighlights[i].transform.position = targets[i].realPosition;
            targetHighlights[i].SetActive(true);
        }
    }

    public void HideTargetHighlights()
    {
        // Don't do anything if the widgets don't exist yet
        if (targetHighlights.Count < 1)
        {
            // Break out of the function
            return;
        }

        // Loop through and disable every widget
        foreach (GameObject targetHighlight in targetHighlights)
        {
            targetHighlight.SetActive(false);
        }
    }
}
