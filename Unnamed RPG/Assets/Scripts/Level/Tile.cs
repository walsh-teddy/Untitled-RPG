using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    protected bool selected = false;
    protected Vector2 position;
    protected float height;

    [SerializeField] protected string displayName;

    [Header("Movement")]
    [SerializeField] float baseMovementCost = 1; // Cost required to move out of it, not into it

    [Header("Color")]
    [SerializeField] Gradient heightGradiant;
    [SerializeField] float maxHeight;
    [SerializeField] float outlineBrightnessPercent; // Between 0.0 and 1.0
    [SerializeField] MeshRenderer body;
    [SerializeField] MeshRenderer outline;

    // Game Objects
    [Header("Highlight Planes")]
    [SerializeField] MeshRenderer lightHighlightPlane;
    [SerializeField] MeshRenderer mediumHighlightPlane;
    [SerializeField] MeshRenderer heavyHighlightPlane;

    Obstacle obstacle;
    Creature occupant;
    LevelSpawner levelSpawner;

    List<TileConnection> connections = new List<TileConnection> { };

    // Properties
    public int x
    {
        get { return (int)position.x; }
    }
    public int y
    {
        get { return (int)position.y; }
    }
    public Vector2 TilePosition
    {
        get { return new Vector2(x, y); }
    }
    public Vector3 RealPosition
    {
        get { return gameObject.transform.position; }
    }
    public highlightLevels Highligted
    {
        set 
        {
            // Turn on / off the correct highlight plane depending on the level
            switch (value)
            {
                case highlightLevels.none:
                    lightHighlightPlane.enabled = false;
                    mediumHighlightPlane.enabled = false;
                    heavyHighlightPlane.enabled = false;
                    break;
                case highlightLevels.light:
                    lightHighlightPlane.enabled = true;
                    mediumHighlightPlane.enabled = false;
                    heavyHighlightPlane.enabled = false;
                    break;
                case highlightLevels.medium:
                    lightHighlightPlane.enabled = false;
                    mediumHighlightPlane.enabled = true;
                    heavyHighlightPlane.enabled = false;
                    break;
                case highlightLevels.heavy:
                    lightHighlightPlane.enabled = false;
                    mediumHighlightPlane.enabled = false;
                    heavyHighlightPlane.enabled = true;
                    break;
            }
        }
    }
    public bool HasObstacle
    {
        get { return obstacle != null; }
    }
    public Obstacle Obstacle
    {
        get { return obstacle; }
    }
    public bool HasOccupant
    {
        get { return occupant != null; }
    }
    public Creature Occupant
    {
        get { return occupant; }
        set { occupant = value; }
    }
    public bool IsOpen
    {
        get { return obstacle == null && occupant == null; }
    }
    public string DisplayName
    {
        get { return displayName; }
    }
    public float Height
    {
        get { return height; }
    }
    public float TopHeight
    {
        // Return the tallest point on the tile (either its base or the top point of anything thats in it)
        get
        {
            if (HasObstacle) // There is an obstacle in it
            {
                return height + obstacle.Height;
            }
            else if (HasOccupant) // There is a creature in it
            {
                return height + occupant.Height;
            }
            else // It is empty
            {
                return height;
            }
        }
    }
    public List<TileConnection> Connections
    {
        get { return connections; }
    }
    public Gradient HeightGradiant
    {
        get { return heightGradiant; }
    }

    public void Create(int x, int y, float height)
    {
        position = new Vector2(x, y);
        this.height = height;
        levelSpawner = GameObject.FindGameObjectWithTag("GameManager").GetComponent<LevelSpawner>();
        gameObject.name = ToString();

        UpdateColor();
    }
    public void Create(int x, int y, float height, Obstacle obstacle)
    {
        Create(x, y, height);
        this.obstacle = obstacle;
        obstacle.Create(this);
    }
    public void Create(int x, int y, float height, Creature occupant)
    {
        Create(x, y, height);
        this.occupant = occupant;
        this.occupant.Create(this);
    }
    // Cache a list of all connections between this and other tiles
    // called after the map is created
    public void CalculateConnections()
    {
        // TODO: Look at a map file for different connection dinstances (like vaulting over cover would be 1 extra tile)
        // This may need to happen while the tiles are being created and the list is updated every tile

        // Get a list of adjasent tiles
        foreach (Tile tile in levelSpawner.AdjacentTiles(this))
        {
            float length = baseMovementCost;
            // Check if its diagonal
            if (tile.x != x && tile.y != y) // Its diagonal
            {
                // Moving diagonally costs more
                length += 0.5f;
            }
            // Check if its higher
            if (tile.Height > height) // The new tile is higher
            {
                // Add half the height difference as length 
                length += 0.5f* (tile.Height - height);
            }
            connections.Add(new TileConnection(tile, length));
        }
    }
    protected void UpdateColor()
    {
        // Alter the color of the tile
        Color tileColor = heightGradiant.Evaluate(height / maxHeight);
        body.material.SetColor("_Color", tileColor);
        // The outline should be darker
        tileColor.r *= outlineBrightnessPercent;
        tileColor.g *= outlineBrightnessPercent;
        tileColor.b *= outlineBrightnessPercent;
        outline.material.SetColor("_Color", tileColor);
    }

    // Change the height of the tile in the level editor
    public void AdjustHeight(float heightChange)
    {
        // Adjust the height and restrain it to min/max
        float changeToHeight = height + heightChange;
        if (changeToHeight < 0)
        {
            changeToHeight = 0;
        }
        else if (changeToHeight > maxHeight)
        {
            changeToHeight = maxHeight;
        }

        // Adjust the physical height of the tile and the actual value
        gameObject.transform.Translate(Vector3.up * (changeToHeight - height));
        height = changeToHeight;

        // Update to the new color
        UpdateColor();
    }

    // Change the name and color to match a different terrain type in the level editor
    public void AdjustTerrainType(string displayName, Gradient heightGradiant)
    {
        // Connection weight doesn't matter for this, since this is only called in the level editor
        this.displayName = displayName;
        this.heightGradiant = heightGradiant;

        // Display the change
        UpdateColor();
    }

    // Change the detail shown on this tile (will always be an obstacle)
    public void AdjustDetail(GameObject detailPrefab)
    {
        // Test if it should be adding a new detail or removing one
        if (detailPrefab != null) // Add a detail
        {
            // Remove any detail already there
            if (HasObstacle) // There was already a detail
            {
                // Delete it
                GameObject.Destroy(obstacle.gameObject);
            }

            // Create the new detail and store it
            obstacle = Instantiate(detailPrefab, gameObject.transform).GetComponent<Obstacle>();
            obstacle.Create(this);
        }
        else // Remove a detail if there is one
        {
            // Make sure there is actually a detail to be removed
            if (HasObstacle) // There is a detail
            {
                // Delete it
                GameObject.Destroy(obstacle.gameObject);
                obstacle = null;
            }
        }
    }

    // Return the connection length between this and the entered tile
    public float ConnectionLength(Tile tile)
    {
        // Loop through connections
        foreach (TileConnection connection in connections)
        {
            if (connection.tile == tile) // This is the tile that was entered
            {
                return connection.length;
            }
        }

        Debug.LogError("Tile.ConnectionLength() called with a tile that is not connected");
        return 0;
    }

    public bool IsConnected(Tile tile)
    {
        // Loop through each connection
        foreach (TileConnection connection in connections)
        {
            // Test if this is connected to the tile we're looking for
            if (connection.tile == tile) // It is the tile we're looking for
            {
                return true;
            }
        }

        // We never returned true and therefor, we never found the tile
        return false;
    }

    public override string ToString()
    {
        return ("Tile," + displayName + "(" + x + "/" + y + ")");
    }
}

public struct TileConnection
{
    public Tile tile;
    public float length;

    public TileConnection(Tile tile, float length)
    {
        this.tile = tile;
        this.length = length;
    }
}