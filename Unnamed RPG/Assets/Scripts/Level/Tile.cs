using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    bool selected = false;
    Vector2 position;
    float height;

    // Game Objects
    [SerializeField] MeshRenderer lightHighlightPlane;
    [SerializeField] MeshRenderer mediumHighlightPlane;
    [SerializeField] MeshRenderer heavyHighlightPlane;
    public enum highlightLevels { none, light, medium, heavy }
    Obstacle obstacle;
    Creature occupant;

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
    public Vector3 realPosition
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


    public void Create(int x, int y, float height)
    {
        position = new Vector2(x, y);
        this.height = height;
    }
    public void Create(int x, int y, float height, Obstacle obstacle)
    {
        position = new Vector2(x, y);
        this.height = height;
        this.obstacle = obstacle;
    }
    public void Create(int x, int y, float height, Creature occupant)
    {
        position = new Vector2(x, y);
        this.height = height;
        this.occupant = occupant;
        this.occupant.Create(this);
    }
}
