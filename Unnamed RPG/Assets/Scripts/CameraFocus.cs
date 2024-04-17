using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFocus : MonoBehaviour
{
    // Reference variables
    public Camera camera;
    private GameObject gameManager;
    LevelSpawner levelSpawner;
    Game game;
    PlayerManager playerManager;
    float dt;

    // Control Variables
    PlayerInput playerInput;
    private InputAction moveControl;
    private InputAction zoomControl;
    private InputAction rotateControl;
    private InputAction changeFocusControl;
    private InputAction changePerspectiveControl;
    private InputAction cyclePlayersForwardControl;
    private InputAction cyclePlayersBackwardsControl;
    private InputAction FocusSelectedPlayerControl;

    // Movement Variables
    [Header("Movement Variables")]
    [SerializeField] private float moveSpeed = 8;
    [SerializeField] private float catchUpSpeed = 4;
    private Vector3 realPosition;
    private Vector2 targetPosition;


    // Zoom Variables
    [Header("Zoom Variables")]
    [SerializeField] private float maxCameraDistnace = 20;
    [SerializeField] private float minCameraDistance = 5;
    [SerializeField] private float scrollSpeed = 10;
    private Vector3 directionTowardsCamera;
    public float cameraDistance = 12;


    // Rotation Variables
    [Header("Rotation Variables")]
    [SerializeField] private float rotationSpeed = 90;
    private float rotation = 0; // In degrees
    private Quaternion originalRotation;

    // Camera state
    public enum CameraState { 
        free, // The camera can freely move and rotate
        topDown, // The camera can freely move and is top down orthographic (changeFocus)
        locked, // The camera is stuck in 1 place or moving somewhere and the player is unable to move or rotate it
        perspective // The camera is from someone's perspective and can rotate but not move
    }

    CameraState currentState = CameraState.free;

    private void Awake()
    {
        playerInput = new PlayerInput();

        // Get the value for the level spawner
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        levelSpawner = gameManager.GetComponent<LevelSpawner>();
        game = gameManager.GetComponent<Game>();
    }

    private void OnEnable()
    {
        // Move Control (WASD)
        moveControl = playerInput.Player.Move;
        moveControl.Enable();

        // Zoom Control (Scroll wheel)
        zoomControl = playerInput.Player.Zoom;
        zoomControl.Enable();

        // Rotate Control (Q & E)
        rotateControl = playerInput.Player.Rotate;
        rotateControl.Enable();

        // Change Focus Control (R)
        changeFocusControl = playerInput.Player.ChangeFocus;
        changeFocusControl.Enable();
        changeFocusControl.performed += ChangeFocus;

        // Change Perspective Control (T)
        changePerspectiveControl = playerInput.Player.ChangePerspective;
        changePerspectiveControl.Enable();
        changePerspectiveControl.performed += ChangePerspective;

        // Cycle players forward (Tab)
        cyclePlayersForwardControl = playerInput.Player.CyclePlayersForward;
        cyclePlayersForwardControl.Enable();
        cyclePlayersForwardControl.performed += CyclePlayersForward;

        // Cycle players backwards (L-Shift)
        cyclePlayersBackwardsControl = playerInput.Player.CyclePlayersBackwards;
        cyclePlayersBackwardsControl.Enable();
        cyclePlayersBackwardsControl.performed += CyclePlayersBackwards;

        // Focus Selected Player (Space)
        FocusSelectedPlayerControl = playerInput.Player.FocusSelectedPlayer;
        FocusSelectedPlayerControl.Enable();
        FocusSelectedPlayerControl.performed += FocusSelectedPlayer;
    }

    private void OnDisable()
    {
        moveControl.Disable();
        zoomControl.Disable();
        rotateControl.Disable();
        changeFocusControl.Disable();
        changePerspectiveControl.Disable();
        cyclePlayersForwardControl.Disable();
        cyclePlayersBackwardsControl.Disable();
        FocusSelectedPlayerControl.Disable();
    }
    
    void Start()
    {
        playerManager = game.PlayerManager;

        // Store initial positions and rotations
        directionTowardsCamera = (camera.transform.position - transform.position).normalized;
        originalRotation = camera.transform.rotation;

        // Save the curent position as the position
        targetPosition = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        realPosition = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Make it smoother
        dt = Time.deltaTime;
        Move();
        Zoom();
        Rotate();
        UpdateWorldTransform();
    }

    // Slowly move the camera to a spesific point
    public void MoveTo (Creature target)
    {
        // Move the pointer
        targetPosition.x = target.Space.RealPosition.x;
        targetPosition.y = target.Space.RealPosition.z;
    }

    void Move()
    {
        // Only do this if controls are enabled
        switch (currentState)
        {
            case CameraState.free:
            case CameraState.topDown:

                Vector2 movement = Quaternion.Euler(0, 0, -rotation) * moveControl.ReadValue<Vector2>().normalized * moveSpeed * dt;
                Vector2 newTargetPosition = targetPosition + movement;

                // Clamp in the target location so it doesnt go off the map
                if (newTargetPosition.x < levelSpawner.RealLeft) // Its too far to the left
                {
                    newTargetPosition.x = levelSpawner.RealLeft;
                }
                else if (newTargetPosition.x > levelSpawner.RealRight) // its too far to the right 
                {
                    newTargetPosition.x = levelSpawner.RealRight;
                }
                if (newTargetPosition.y < levelSpawner.RealBottom) // Its too far down
                {
                    newTargetPosition.y = levelSpawner.RealBottom;
                }
                else if (newTargetPosition.y > levelSpawner.RealTop) // Its too far up
                {
                    newTargetPosition.y = levelSpawner.RealTop;
                }

                // Now that we are confident that target has valid data, update targetPosition
                targetPosition = newTargetPosition;

                break;
        }
    }

    void Zoom()
    {
        // Only do this if controls are enabled
        switch (currentState)
        {
            case CameraState.free:
            case CameraState.topDown:

            float zoomChange = zoomControl.ReadValue<float>() * scrollSpeed * dt;
            cameraDistance += zoomChange;

            // Clamp camera distance
            if (cameraDistance < minCameraDistance) // Too close
            {
                cameraDistance = minCameraDistance;
            }
            else if (cameraDistance > maxCameraDistnace) // Too far
            {
                cameraDistance = maxCameraDistnace;
            }

            break;
        }
    }

    void Rotate()
    {
        // Only do this if controls are enabled
        switch (currentState)
        {
            case CameraState.free:
            case CameraState.topDown:

                rotation += rotateControl.ReadValue<float>() * rotationSpeed * dt;

                break;

            case CameraState.perspective: // Rotation is backwards in perspective

                rotation -= rotateControl.ReadValue<float>() * rotationSpeed * dt;

                break;
        }
    }

    void ChangeFocus(InputAction.CallbackContext context)
    {
        // Only do this if controls are enabled
        switch (currentState)
        {
            // Switch between free and topDown
            case CameraState.free:

                camera.orthographic = true;
                currentState = CameraState.topDown;

                break;

            case CameraState.topDown:

                camera.orthographic = false;
                currentState = CameraState.free;

                break;
        }
    }

    // View the map from the perspective of the selected player or exit that state
    void ChangePerspective(InputAction.CallbackContext cotext)
    {
        // Only do this if a player is selected with no abilities
        if (!(game.CurrentState == Game.gameState.playerSelected ||
            game.CurrentState == Game.gameState.playerSelectedSubmitted))
        {
            return;
        }


        switch (currentState)
        {
            case CameraState.free:
            case CameraState.topDown:

                // Switch to perspective state
                currentState = CameraState.perspective;

                // Turn off the mesh for the target player
                playerManager.SelectedPlayer.Body.SetActive(false);

                // Move camera to target player's location
                realPosition = playerManager.SelectedPlayer.Space.RealPosition;
                targetPosition = new Vector2(playerManager.SelectedPlayer.Space.RealPosition.x, playerManager.SelectedPlayer.Space.RealPosition.z);

                break;

            case CameraState.perspective:

                LeavePerspective();

                break;
        }

    }

    // Exit out of perspective / topdown mode (can be called by Game.cs when the gameState changes)
    public void LeavePerspective()
    {
        // Set the state back to free
        currentState = CameraState.free;

        // Set the camera to non orthographic (incase it was orthographic)
        camera.orthographic = false;

        // Turn the player mesh back on (incase it was off)
        playerManager.SelectedPlayer.Body.SetActive(true);
    }

    void CyclePlayersForward(InputAction.CallbackContext context)
    {
        playerManager.CyclePlayers(1);
    }

    void CyclePlayersBackwards(InputAction.CallbackContext context)
    {
        playerManager.CyclePlayers(-1);
    }

    void FocusSelectedPlayer(InputAction.CallbackContext context)
    {
        // Don't do anything if no player is selected
        if (game.CurrentState == Game.gameState.nothingSelected) { return; }

        MoveTo(playerManager.SelectedPlayer);
    }

    void UpdateWorldTransform()
    {
        // Update the height of the target position to match the tile it is over
        float tileHeight = 0;
        Tile targetTile = levelSpawner.TargetTile(targetPosition);
        if (targetTile != null)
        {
            tileHeight = targetTile.Height;
        }

        // Update the real position to move towards the target position
        realPosition += (new Vector3(targetPosition.x, tileHeight, targetPosition.y) - realPosition) * dt * catchUpSpeed;

        // Move the object to its new location
        transform.position = realPosition;

        // Reset camera rotation
        camera.transform.rotation = originalRotation;

        // Draw orthographically pointing strait down if its topdown
        switch (currentState)
        {
            case CameraState.free:
            case CameraState.locked:

                // Update Position (slightly offset from the target location)
                camera.transform.position = transform.position + directionTowardsCamera * cameraDistance;

                // Rotate around axis
                camera.transform.RotateAround(transform.position, Vector3.up, rotation);

                break;

            case CameraState.topDown:

                // Update position (directly above the curent location)
                camera.transform.position = new Vector3(realPosition.x, 100, realPosition.z);

                // Update the rotation (zoomed in/out of the curent location)
                camera.transform.rotation = Quaternion.identity;
                camera.transform.RotateAround(camera.transform.position, Vector3.right, 90f);
                camera.transform.RotateAround(camera.transform.position, Vector3.up, rotation);

                // Update orthographic zoom
                camera.orthographicSize = cameraDistance / 2;

                break;

            case CameraState.perspective:

                // Update position (in the eyespace of the target player)
                camera.transform.position = new Vector3(
                    playerManager.SelectedPlayer.Space.RealPosition.x,
                    playerManager.SelectedPlayer.EyeHeight + playerManager.SelectedPlayer.Space.Height,
                    playerManager.SelectedPlayer.Space.RealPosition.z
                );

                // Look horrizontal
                camera.transform.rotation = Quaternion.identity;
                //camera.transform.RotateAround(camera.transform.position, Vector3.right, 90f);

                // Rotate around axis
                camera.transform.RotateAround(playerManager.SelectedPlayer.Space.RealPosition, Vector3.up, rotation);

                break;
        }

    }
}
