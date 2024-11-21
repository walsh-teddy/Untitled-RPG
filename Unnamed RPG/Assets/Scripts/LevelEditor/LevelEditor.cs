using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

// Create a flat layout of tiles and allow pointer.cs to alter them
public class LevelEditor : LevelSpawner
{
    [Header("Level Editor")]
    [SerializeField] int mapHeight;
    [SerializeField] int mapWidth;

    [Header("Prefabs")]
    [SerializeField] GameObject rockPrefab;
    [SerializeField] GameObject playerSpawnPlaceholder;
    [SerializeField] GameObject enemySpawnPlaceholder;

    // TODO: Get rid of these
    public Tile BasicTile
    {
        get { return prefabContainer.TilePrefabs["Grass"].GetComponent<Tile>(); }
    }
    public Tile SlowTile
    {
        get { return prefabContainer.TilePrefabs["Mud"].GetComponent<Tile>(); }
    }

    protected override void CalculateMapEdges()
    {
        tileHeight = mapHeight;
        tileWidth = mapWidth;
        base.CalculateMapEdges();
    }

    public override void SpawnLevel()
    {
        // Initialize empty array
        map = new Tile[mapHeight, mapWidth];

        // Create a tile for each
        // Create the actual grid of tiles
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Create a tile at height = 0
                // TODO: Allow for a different default tile prefab probably
                map[x, y] = Instantiate(prefabContainer.TilePrefabs["Grass"], new Vector3(x * tileSize, 0, y * tileSize), Quaternion.identity).GetComponent<Tile>();
                map[x, y].Create(x, y, 0);
            }
        }

        CalculateMapEdges();
    }

    public void SpawnDetail(Tile tile, DetailType detailType)
    {
        switch (detailType)
        {
            case DetailType.Obstacle: // Rock
                // TODO: Allow for different obstacles ("Rock" may not exist, either)
                tile.AdjustDetail(rockPrefab);
                break;

            case DetailType.Player: // Player spawn
                tile.AdjustDetail(playerSpawnPlaceholder);
                break;

            case DetailType.Enemy: // Enemy spawn
                tile.AdjustDetail(enemySpawnPlaceholder);
                break;
        }
    }

    public void RemoveDetail(Tile tile)
    {
        tile.AdjustDetail(null);
    }

    // copy all information of the tiles into a text file
    public void SaveLevel()
    {
        // Following this tutorial: https://www.youtube.com/watch?v=iFJeg9AzN2Y

        // Create the folder if there isn't one yet
        string mapFolder = Application.streamingAssetsPath + "/Maps/";
        Directory.CreateDirectory(mapFolder);

        string text = ""; // Write to this text
        // X and Y bounds for the map
        text += mapWidth + "/" + mapHeight + "\n";

        // TODO: Have each map file use 1 grid of info, rather than looping through each tile multiple times

        // Height values
        for (int x = 0; x < mapWidth; x ++)
        {
            for (int y = 0; y < mapHeight; y ++)
            {
                // Add this tile's height, seperated by a comma
                text += map[x, y].Height + ",";
            }

            // Go to a new line
            text += "\n";
        }

        text += "\n";

        // Tile Type
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Add this tile's type, seperated by a comma
                text += map[x, y].DisplayName + ",";
            }

            // Go to a new line
            text += "\n";
        }

        text += "\n";

        // Detail
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Test if the tile is empty or not
                if (map[x,y].HasObstacle)
                {
                    text += map[x, y].Obstacle.DisplayName + ",";
                }
                else
                {
                    text += "_,";
                }
            }

            // Go to a new line
            text += "\n";
        }

        // Save over any text that was there before
        // TODO: Allow for multiple different maps with different names
        string documentPath = mapFolder + "Map" + ".txt";
        File.WriteAllText(documentPath, text);
    }
}