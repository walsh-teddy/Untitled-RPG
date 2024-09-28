using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] string displayName;
    [SerializeField] float height;

    // Game manager variables
    protected GameObject gameManager;
    protected LevelSpawner levelSpawner;
    Tile space;

    // Properties
    public Tile Space
    {
        get { return space; }
    }
    public string DisplayName
    {
        get { return displayName; }
    }
    public float Height
    {
        get { return height; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public virtual void Create(Tile space)
    {
        // Get the value for the level spawner
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        levelSpawner = gameManager.GetComponent<LevelSpawner>();

        // Attatch to the tile
        this.space = space;

        gameObject.transform.position = space.RealPosition;
    }

    public override string ToString()
    {
        return ("Obstacle," + displayName);
    }
}
