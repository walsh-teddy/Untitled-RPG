using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Pointer : MonoBehaviour
{
    // Variables
    private Vector3 screenPosition;
    private Camera mainCamera;
    LevelSpawner levelSpawner;
    UIManager uiManager;
    PlayerManager playerManager;
    Game game;

    // Only used if this is a level editor
    LevelEditorManager levelEditorManager;

    [Header("Raycasting")]
    [SerializeField] LayerMask layersToHit;
    [SerializeField] LayerMask layersToIgnore;
    [SerializeField] GraphicRaycaster graphicRaycaster; // Raycaster used for the UI
    const float HIGH_NUMBER = 1000;

    Tile hoveringTile; // The tile the player is curently hovering over
    Tile aoeTargetTile; // Only used in Update() when updating to find an AOE
    bool aoeTargetLocked = false;
    public bool AOETargetLocked
    {
        get { return aoeTargetLocked; }
        set { aoeTargetLocked = value; }
    }

    // Control Variables
    [SerializeField] PlayerInput playerInput;
    private InputAction leftClickControl;
    private InputAction rightClickControl;

    // State variables

    // Making this once so it doesn't need to be made every frame
    List<Tile> emptyListOfTiles = new List<Tile> { };
    List<Tile> hoveringTileInAList = new List<Tile> { new Tile() };
    private void Awake()
    {
        playerInput = new PlayerInput();

        // Get the value for other scripts
        levelSpawner = gameObject.GetComponent<LevelSpawner>();
        uiManager = gameObject.GetComponent<UIManager>();
        game = gameObject.GetComponent<Game>();

        // Save the camera
        mainCamera = Camera.main;

        // Only used if this is a level editor
        try
        {
            levelEditorManager = (LevelEditorManager)game;
        }
        catch
        {
            // Do nothing lol
        }
    }

    private void Start()
    {
        playerManager = game.PlayerManager;
    }

    private void OnEnable()
    {
        // Left Click
        leftClickControl = playerInput.Player.LeftClick;
        leftClickControl.Enable();
        leftClickControl.performed += LeftClick;

        // Right Click
        rightClickControl = playerInput.Player.RightClick;
        rightClickControl.Enable();
        rightClickControl.performed += RightClick;
    }
    private void OnDisable()
    {
        leftClickControl.Disable();
        rightClickControl.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        // Raycast to the tile map
        screenPosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Vector3 worldPosition = new Vector3(0, HIGH_NUMBER, 0);

        // Check if it is clickin on the UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            hoveringTile = null;
            return;
        }
        else if (Physics.Raycast(ray, out RaycastHit worldHitData, 100, layersToHit)) // Its pointing at the map
        {
            worldPosition = worldHitData.point;
            hoveringTile = levelSpawner.TargetTile(worldPosition);
            hoveringTileInAList[0] = hoveringTile;
        }

        // Make sure its pointing at something
        if (aoeTargetTile != null && hoveringTile != null && worldPosition.y != HIGH_NUMBER)
        {

            switch (game.CurrentState)
            {

                case gameState.playerActionSelectTarget:

                    // Save the selected action (for ease of reference)
                    Action selectedAction = playerManager.SelectedPlayer.SelectedAction;

                    // Update the area every frame if its an AOE. Otherwise highlight the hover tile if its within range
                    if (selectedAction.TargetType == targetTypes.aoe && !aoeTargetLocked) // The action is an AOE and the target is not locked yet
                    {
                        if (selectedAction.TargetsLocked)
                        {
                            aoeTargetLocked = true;
                        }

                        // Update the AOE to point at the hovering tile
                        selectedAction.SetTarget(hoveringTile);

                        // Highlight the new tiles
                        levelSpawner.HighlightTiles(
                            selectedAction.PossibleSpaces, // Light
                            selectedAction.PossibleTargets, // Medium
                            selectedAction.AOETilesWithCreatures // Heavy
                        );
                    }
                    else if ((selectedAction.TargetType == targetTypes.single ||
                        selectedAction.TargetType == targetTypes.move) &&
                        !selectedAction.TargetsLocked) // The action is not an AOE and the targets are not locked
                    {
                        // Darken the tile that the mouse is hovering over if its valid
                        if (selectedAction.PossibleTargets.Contains(hoveringTile)) // It is a valid next step
                        {
                            // Heavy highlight the hovering tile
                            levelSpawner.HighlightTiles(
                                hoveringTileInAList // Heavy Highlight
                            );
                        }
                        else if (selectedAction.PossibleSpaces.Contains(hoveringTile) && selectedAction.TargetType == targetTypes.move) // Hovering over a possible space for an ability where that should be highlighted
                        {
                            levelSpawner.HighlightTiles(
                                hoveringTileInAList // Heavy Highlight
                            );
                        }
                        else // The hovering tile is not one of the valid moves
                        {
                            // Don't heavy highlight any tiles
                            levelSpawner.UnHighlightHeavyTiles();
                        }
                    }

                    break;

                case gameState.editingMap:
                    switch (levelEditorManager.BrushType)
                    {
                        case BrushType.Terraform:
                        case BrushType.Rough:
                            // Heavy highlight the hovering tile
                            levelEditorManager.Brush.SetTarget(hoveringTile);

                            levelSpawner.HighlightTiles(
                                levelEditorManager.Brush.PossibleTargets // Heavy Highlight
                            );
                            break;

                        case BrushType.Details:
                            // Only highlight the 1 tile
                            levelSpawner.HighlightTiles(hoveringTileInAList);
                            break;
                    }
                    break;
            }
        }
        else
        {
            // Origin tile is only used for AOE stuff
            aoeTargetTile = hoveringTile;
            // levelSpawner.SelectTiles(emptyListOfTiles);
        }

        transform.position = worldPosition;
    }

    private void LeftClick(InputAction.CallbackContext context)
    {
        // Don't do anything if the game is uninteractable
        if (game.CurrentState == gameState.uninteractable) // Its uninteractable
        {
            // Break out of the function
            return;
        }

        // Make sure the mouse is pointing at the map and not the UI
        if (hoveringTile == null) // its pointing at the UI
        {
            // End function early
            return; 
        }

        // Do something different depending on the game state
        switch (game.CurrentState)
        {
            // Select the player being clicked on
            case gameState.nothingSelected:
            case gameState.playerSelected:

                // If selected a player, change the state
                if (hoveringTile.HasOccupant)
                {
                    if (hoveringTile.Occupant.Team == "player")
                    {
                        // Update the selected player
                        playerManager.SelectedPlayer = hoveringTile.Occupant.GetComponent<Player>();

                        // Update the game state
                        game.CurrentState = gameState.playerSelected;
                    }
                }
                break;

            case gameState.playerActionSelectTarget:

                Action selectedAction = playerManager.SelectedPlayer.SelectedAction;

                // Making sure the target selected is valid is done in SetTarget() for each action
                // Test if the attack is an AOE or normal attack
                if (selectedAction.TargetType == targetTypes.aoe) // Its an AOE attack
                {
                    // Lock the AOE target tile and set the target as that
                    aoeTargetTile = hoveringTile;
                    aoeTargetLocked = true;
                    selectedAction.SetTarget(aoeTargetTile);
                }
                else // Its not an AOE
                {
                    // Set the target as the hovering tile
                    selectedAction.SetTarget(hoveringTile);
                }
                break;

            // In any state where trying to find an AOE, leftclicking should select the origin tile (if there is one)

            case gameState.editingMap:
                levelEditorManager.Paint(hoveringTile);
                break;

            default:
                aoeTargetTile = hoveringTile;
                break;
        }

        // Update the UI
        uiManager.UpdateUI();
    }

    private void RightClick(InputAction.CallbackContext context)
    {
        // Don't do anything if the game is uninteractable
        if (game.CurrentState == gameState.uninteractable) // Its uninteractable
        {
            // Break out of the function
            return;
        }


        switch (game.CurrentState)
        {
            case gameState.editingMap:
                levelEditorManager.Reduce(hoveringTile);
                break;

            default:
                // Unlock the AOE target if it was locked
                if (aoeTargetLocked) // It was AOE target locked
                {
                    aoeTargetLocked = false;
                    uiManager.HideTargetHighlights();
                }
                else // The AOE target was not locked
                {
                    // Go back 1 step in the menu
                    playerManager.BackButtonClicked();
                }
                break;
        }
    }
}
