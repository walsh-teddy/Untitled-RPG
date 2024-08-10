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

    [SerializeField] private float deltaTimeLimit = 0.5f;

    // Movement Variables
    [Header("Movement Variables")]
    [SerializeField] private float moveSpeed = 8;
    [SerializeField] private float catchUpMoveSpeed = 4;
    private Vector3 realPosition;
    private Vector2 targetPosition;


    // Zoom Variables
    [Header("Zoom Variables")]
    [SerializeField] private float maxCameraDistnace = 20;
    [SerializeField] private float minCameraDistance = 5;
    [SerializeField] private float zoomSpeed = 10;
    [SerializeField] private float catchUpZoonSpeed = 7;
    [SerializeField] private float zoomBufferPercent = 1.2f; // (1.0 = 100%) What percentage of the distance between positions should be shown in ShowPosition
    private Vector3 directionTowardsCamera;
    protected float realCameraDistance = 12;
    protected float targetCameraDistance = 12;


    // Rotation Variables
    [Header("Rotation Variables")]
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float catchUpRotationSpeed;
    private float realRotation = 0; // In degrees
    private float targetRotation = 0;
    private float realVerticalRotation = 0; // Only used for looking up and down in perspective
    private float targetVerticalRotation = 0;
    private Quaternion originalRotation;

    // Cache variables to fill in values for and plug in
    Vector3 cachedPosition = new Vector3 { };

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
        dt = Time.deltaTime;
        // Cap delta time to stop big movement during lag spikes
        if (dt > deltaTimeLimit)
        {
            // In a single frame, the camera focus will only move as far as this many seconds of movement
            dt = deltaTimeLimit;
        }

        Move();
        Zoom();
        Rotate();
        UpdateWorldTransform();
    }

    // Slowly move the camera to a spesific point
    public void MoveTo(float x, float y)
    {
        targetPosition.x = x;
        targetPosition.y = y;
    }
    public void MoveTo(Vector3 worldSpace)
    {
        // targetPosition uses X and Y, but it coresponds to the X-Z plane in worldspace
        MoveTo(worldSpace.x, worldSpace.z);
    }
    public void MoveTo(Creature target)
    {
        MoveTo(target.Space.RealPosition);
    }

    // Change the camera's zoom to a specific level
    public void ZoomTo(float cameraDistance)
    {
        targetCameraDistance = cameraDistance;
    }

    public void RotateTo(float degrees)
    {
        // TODO: Add logic for determining which direction would be shorter to rotate to
        if (realRotation - degrees > 180) // It would be faster to go counterclockwise than clockwise
        {
            // Adjust targetRotation so that it is above realRotation (and will rotate counterclockwise)
            degrees += 360;
        }

        targetRotation = degrees;
    }

    // Adjust for rotating by 360
    public float DiffBetweenAngles(float angle1, float angle2)
    {
        // Find clockwise and counterclockwise angles
        float clockwise = angle1 - angle2;
        float counterClockwise = angle2 - angle1;

        // Adjust either angle to be within 0 - 360 degrees
        if (clockwise < 0)
        {
            clockwise += 360;
        }
        if (counterClockwise < 0)
        {
            counterClockwise += 360;
        }

        // Test which angle is shorter
        if (clockwise < counterClockwise) // Clockwise is shorter
        {
            return clockwise;
        }
        else // CounterClockwise is shorter
        {
            return counterClockwise;
        }
    }

    // Change the position and zoom of the camera to show 2 points
    public void ShowPosition(List<Vector3> positions, bool focusOnAction)
    {
        // Find the average position
        Vector3 averagePosition = Vector3.zero;
        foreach (Vector3 position in positions)
        {
            averagePosition += position;
        }
        averagePosition /= positions.Count;

        MoveTo(averagePosition);

        // Find the longest distance from the average position that the camera should zoom out to show
        float longestDist = 0;
        foreach (Vector3 position in positions)
        {
            if ((averagePosition - position).magnitude > longestDist)
            {
                longestDist = (averagePosition - position).magnitude;
            }
        }
        longestDist *= zoomBufferPercent;

        // Zoom the camera
        // Triginometry incomming!
        //      Make a triangle showing the FOV stretching from the perspective point towards the focus point and cut it in half
        //      Angle = FOV / 2 (probably 30 degrees)
        //      Oposite = Distance / 2 (also including some buffer)
        //      Adjasent = targetCameraDistance (solve for this)
        //      Tan(Angle) = Oposite / Adjasent
        //      Adjasent = Oposite / Tan(Angle)
        //      targetCameraDistance = (distance/2) / (Tan(FOV/2))

        // Use the vertical FOV rather than horizontal FOV if the action is not focused
        float FOV = camera.fieldOfView * Mathf.Deg2Rad;
        if (!focusOnAction) // The action is between many points and is not focused narrowly
        {
            // Use the vertical FOV, rather than horizontal
            FOV /= camera.aspect;
        }
        ZoomTo((longestDist) / Mathf.Tan(FOV / 2));

        // Rotate to show positions on both sides of the screen if there are only 2 positions
        if (focusOnAction) // The action is between 2 creatures, and this should focus on them
        {
            // Rotate the camera
            // More trig incomming :(
            //      Angle of a vector is ArchTan(X/Y) (X and Y being the components of the vector)
            //      These vector3 use Z as Y in this case
            //      Rotate 90 degrees to make it perpendicular

            // Rotate the closer angle
            Vector3 line = positions[0] - positions[1];
            float lineRot = Mathf.Atan(line.x / line.z) * Mathf.Rad2Deg;

            // Find the closer perpendicular angle (+90 or -90)
            if (DiffBetweenAngles(lineRot + 90, realRotation) < DiffBetweenAngles(lineRot - 90, realRotation)) // +90 is closer
            {
                RotateTo(lineRot + 90);
            }
            else // -90 is closer
            {
                RotateTo(lineRot - 90);
            }
        }
    }

    public void ShowPosition(Vector3 position1, Vector3 position2)
    {
        ShowPosition(new List<Vector3> { position1, position2 }, true);
    }

    public void ShowPosition(Creature creature1, Creature creature2)
    {
        ShowPosition(creature1.Space.RealPosition, creature2.Space.RealPosition);
    }

    public void ShowPosition(Vector3 position)
    {
        MoveTo(position);
        ZoomTo(6); // Arbitrarilly set the zoom to 6 because that probably works for most things...
    }

    public void ShowPosition(Creature creature)
    {
        ShowPosition(creature.Space.RealPosition);
    }

    void Move()
    {
        // Only do this if controls are enabled
        switch (currentState)
        {
            case CameraState.free:
            case CameraState.topDown:

                Vector2 movement = Quaternion.Euler(0, 0, -realRotation) * moveControl.ReadValue<Vector2>().normalized * moveSpeed * dt;
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

            float zoomChange = zoomControl.ReadValue<float>() * zoomSpeed * dt;
            targetCameraDistance += zoomChange;

            // Clamp target camera distance
            if (targetCameraDistance < minCameraDistance) // Too close
            {
                targetCameraDistance = minCameraDistance;
            }
            else if (targetCameraDistance > maxCameraDistnace) // Too far
            {
                targetCameraDistance = maxCameraDistnace;
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

                targetRotation += rotateControl.ReadValue<float>() * rotationSpeed * dt;

                break;

            case CameraState.perspective: // Rotation is backwards in perspective

                // Update target rotation
                targetRotation -= rotateControl.ReadValue<float>() * rotationSpeed * dt;

                // Also use WASD for rotation controls in perspective
                // Only do this if rotateControl is not being used
                if (rotateControl.ReadValue<float>() == 0) // Rotate control is not being used
                {
                    // Update target rotation with A and D
                    targetRotation += moveControl.ReadValue<Vector2>().x * rotationSpeed * dt;
                }

                // Also allow the player to look up and down using W and S
                targetVerticalRotation -= moveControl.ReadValue<Vector2>().y * rotationSpeed * dt;
                // Clamp the vertical rotations (so you can't look behind you upsidown)
                if (targetVerticalRotation < -90) // Its too low
                {
                    targetVerticalRotation = -90;
                }
                if (targetVerticalRotation > 90) // Its too high
                {
                    targetVerticalRotation = 90;
                }

                break;
        }
    }

    // Switch between 3rd person rotating and top down
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
        if (game.CurrentState != gameState.playerSelected)
        {
            return;
        }

        // Failsafe incase this is running before Start()
        if (playerManager == null)
        {
            playerManager = game.PlayerManager;
        }

        switch (currentState)
        {
            case CameraState.free:
            case CameraState.topDown:

                // Switch to perspective state
                currentState = CameraState.perspective;

                // Turn off the mesh for the target player
                playerManager.SelectedPlayer.Body.SetActive(false);
                playerManager.SelectedPlayer.CreatureCanvas.SetActive(false);

                // Move camera to target player's location
                realPosition = playerManager.SelectedPlayer.Space.RealPosition;
                targetPosition = new Vector2(playerManager.SelectedPlayer.Space.RealPosition.x, playerManager.SelectedPlayer.Space.RealPosition.z);

                // Reset vertical rotation
                targetVerticalRotation = 0;
                realVerticalRotation = 0;
                break;

            case CameraState.perspective:

                LeavePerspective();

                break;
        }

    }

    // Exit out of perspective / topdown mode (can be called by Game.cs when the gameState changes)
    public void LeavePerspective()
    {
        // Failsafe incase this is running before Start()
        if (playerManager == null)
        {
            playerManager = game.PlayerManager;
        }

        // Set the state back to free
        currentState = CameraState.free;

        // Set the camera to non orthographic (incase it was orthographic)
        camera.orthographic = false;

        // Turn the player mesh back on (incase it was off)
        playerManager.SelectedPlayer.Body.SetActive(true);
        playerManager.SelectedPlayer.CreatureCanvas.SetActive(true);
    }

    void CyclePlayersForward(InputAction.CallbackContext context)
    {
        playerManager.CyclePlayers(1);
    }

    void CyclePlayersBackwards(InputAction.CallbackContext context)
    {
        playerManager.CyclePlayers(-1);
    }

    // Called when the player hits spacebar
    void FocusSelectedPlayer(InputAction.CallbackContext context)
    {
        // Don't do anything if no player is selected
        if (game.CurrentState == gameState.nothingSelected) { return; }

        MoveTo(playerManager.SelectedPlayer);

        // Zoom onto the player
        targetCameraDistance = minCameraDistance;

        RotateTo(0);
    }

    // Fill in values to cachedPosition to plug in vector3s without needing to create a new object every frame
    void UpdateCachedPosition(float x, float y, float z)
    {
        cachedPosition.x = x;
        cachedPosition.y = y;
        cachedPosition.z = z;
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
        UpdateCachedPosition(targetPosition.x, tileHeight, targetPosition.y);
        realPosition += (cachedPosition - realPosition) * dt * catchUpMoveSpeed;

        // Update the real zoom to aproach the target zoom
        realCameraDistance += (targetCameraDistance - realCameraDistance) * dt * catchUpZoonSpeed;

        // Update the real rotation to aproach the target rotation
        realRotation += (targetRotation - realRotation) * dt * catchUpRotationSpeed;
        // Clamp rotations (only if target and real are both too far)
        if (targetRotation > 360 && realRotation > 360) // Too high
        {
            targetRotation -= 360;
            realRotation -= 360;
        }
        else if (targetRotation < 0 && realRotation < 0) // Too low
        {
            targetRotation += 360;
            realRotation += 360;
        }

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
                camera.transform.position = transform.position + directionTowardsCamera * realCameraDistance;

                // Rotate around axis
                camera.transform.RotateAround(transform.position, Vector3.up, realRotation);

                break;

            case CameraState.topDown:

                // Update position (directly above the curent location)
                UpdateCachedPosition(realPosition.x, 100, realPosition.z);
                camera.transform.position = cachedPosition;

                // Update the rotation (zoomed in/out of the curent location)
                camera.transform.rotation = Quaternion.identity;
                camera.transform.RotateAround(camera.transform.position, Vector3.right, 90f);
                camera.transform.RotateAround(camera.transform.position, Vector3.up, realRotation);

                // Update orthographic zoom
                camera.orthographicSize = realCameraDistance / 2;

                break;

            case CameraState.perspective:

                // Update position (in the eyespace of the target player) using cachedPosition
                UpdateCachedPosition(
                    playerManager.SelectedPlayer.Space.RealPosition.x, // X
                    playerManager.SelectedPlayer.EyeHeight + playerManager.SelectedPlayer.Space.Height, // Y
                    playerManager.SelectedPlayer.Space.RealPosition.z // Z
                );
                camera.transform.position = cachedPosition;

                // Look horrizontal
                camera.transform.rotation = Quaternion.identity;

                // Rotate around axis
                camera.transform.Rotate(Vector3.up, realRotation);

                // Also, rotate vertically when the player hits W or S
                // Only need to update it in this state
                realVerticalRotation += (targetVerticalRotation - realVerticalRotation) * dt * catchUpMoveSpeed;

                camera.transform.Rotate(Vector3.right, realVerticalRotation);

                break;
        }

    }
}
