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
    private Vector3 worldPosition;
    private Camera camera;
    LevelSpawner levelSpawner;
    UIManager uiManager;
    PlayerManager playerManager;
    Game game;
    public Animator testCubeAnimator;

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
        playerManager = gameObject.GetComponent<PlayerManager>();
        game = gameObject.GetComponent<Game>();

        // Save the camera
        camera = Camera.main;
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
        Ray ray = camera.ScreenPointToRay(screenPosition);
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
                case Game.gameState.playerActionSelectTarget:

                    // Update the area every frame if its an AOE. Otherwise highlight the hover tile if its within range
                    if (playerManager.SelectedPlayer.SelectedAction.IsAOE && !aoeTargetLocked) // The attack is an AOE and the target is not locked yet
                    {
                        // Update the AOE to point at the hovering tile
                        playerManager.SelectedPlayer.SelectedAction.SetTarget(hoveringTile);

                        // Highlight the new tiles
                        levelSpawner.HighlightTiles(
                            playerManager.SelectedPlayer.SelectedAction.PossibleSpaces, // Light
                            playerManager.SelectedPlayer.SelectedAction.PossibleTargets, // Medium
                            playerManager.SelectedPlayer.SelectedAction.AOETilesWithCreatures // Heavy
                        );
                    }
                    else if (!playerManager.SelectedPlayer.SelectedAction.IsAOE) // The attack is not an AOE
                    {
                        // Darken the tile that the mouse is hovering over if its valid
                        if (playerManager.SelectedPlayer.SelectedAction.PossibleTargets.Contains(hoveringTile)) // It is a valid next step
                        {
                            // Heavy highlight the hovering tile
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
            case Game.gameState.nothingSelected:
            case Game.gameState.playerSelected:

                // If selected a player, change the state
                if (hoveringTile.HasOccupant)
                {
                    if (hoveringTile.Occupant.Team == "player")
                    {
                        playerManager.SelectedPlayer = hoveringTile.Occupant.GetComponent<Player>();

                        // Check if they've already submitted an action this round
                        if (!playerManager.SelectedPlayer.HasSubmittedAction) // The player has not submitted an action yet
                        {
                            game.CurrentState = Game.gameState.playerSelected;
                        }
                        else // They have already submitted an action this round
                        {
                            game.CurrentState = Game.gameState.playerSelectedSubmitted;
                        }
                    }
                }
                break;

            case Game.gameState.playerActionSelectTarget:

                // Test if the attack is an AOE or normal attack
                if (playerManager.SelectedPlayer.SelectedAction.IsAOE) // It is an AOE attack
                {
                    aoeTargetTile = hoveringTile;
                    aoeTargetLocked = true;
                    playerManager.SelectedPlayer.SelectedAction.SetTarget(aoeTargetTile);
                }
                else // It is a normal attack
                {
                    // Select the target for that action
                    playerManager.SelectedPlayer.SelectedAction.SetTarget(hoveringTile);
                }

                break;

            // In any state where trying to find an AOE, leftclicking should select the origin tile (if there is one)
            default:
                aoeTargetTile = hoveringTile;
                break;
        }

        // Update the UI
        uiManager.UpdateUI();
    }

    private void RightClick(InputAction.CallbackContext context)
    {
        // Unlock the AOE target if it was locked
        if (aoeTargetLocked) // It was AOE target locked
        {
            aoeTargetLocked = false;
            uiManager.HideTargetHighlights();
        } else // The AOE target was not locked
        {
            // Go back 1 step in the menu
            playerManager.BackButtonClicked();

        }
    }
}
