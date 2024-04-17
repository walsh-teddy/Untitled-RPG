using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    protected bool selected = false;
    protected Vector2 position;
    protected float height;

    // Game Objects
    [SerializeField] MeshRenderer lightHighlightPlane;
    [SerializeField] MeshRenderer mediumHighlightPlane;
    [SerializeField] MeshRenderer heavyHighlightPlane;
    public enum highlightLevels { none, light, medium, heavy }
    Obstacle obstacle;
    Creature occupant;
    LevelSpawner levelSpawner;

    public struct TileConnection
    {
        public Tile tile;
        public float length;

        public TileConnection (Tile tile, float length)
        {
            this.tile = tile;
            this.length = length;
        }
    }
    List<TileConnection> connections = new List<TileConnection> { };

    [SerializeField] string displayName;

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
    public string Name
    {
        get { return displayName; }
    }
    public float Height
    {
        get { return height; }
    }
    public List<TileConnection> Connections
    {
        get { return connections; }
    }

    public void Create(int x, int y, float height)
    {
        position = new Vector2(x, y);
        this.height = height;
        levelSpawner = GameObject.FindGameObjectWithTag("GameManager").GetComponent<LevelSpawner>();
        gameObject.name = ToString();
    }
    public void Create(int x, int y, float height, Obstacle obstacle)
    {
        Create(x, y, height);
        this.obstacle = obstacle;
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
            float length = 1f;
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
