using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LevelSpawner : MonoBehaviour
{
    // Variables
    protected int tileWidth = 10;
    protected int tileHeight = 10;
    [SerializeField] string mapFile;
    public float tileSize = 1;
    protected int tileLeft, tileRight, tileTop, tileBottom;
    protected float realWidth, realHeight;
    protected float realLeft, realRight, realTop, realBottom;
    protected Tile[,] map;
    protected Game game;

    // Used in AOE and range calculations to not check a tile twice per calculation
    struct checkedTiles
    {
        public Vector3 source;
        public Vector3 target;
        public bool valid;

        public checkedTiles(Vector3 source, Vector3 target, bool valid)
        {
            this.source = source;
            this.target = target;
            this.valid = valid;
        }
    }
    List<checkedTiles> tilesBeingChecked = new List<checkedTiles> { };

    // Objects to be created or moved around
    [Header("Prefabs")]
    protected PrefabContainer prefabContainer;
/*    [SerializeField] protected List<GameObject> playerPrefabList; // TODO: Allow for specific player spawns
    [SerializeField] protected List<GameObject> enemyPrefabList; // TODO: Allow for different enemy types
    [SerializeField] protected List<GameObject> obstaclePrefabList;
    [SerializeField] protected List<GameObject> tilePrefabList;

    // Create dictionaries using each of the obstacles and tile prefabs displaNames as the keys (details filled in during Awake())
    protected Dictionary<string, GameObject> obstaclePrefabDic = new Dictionary<string, GameObject> { };
    protected Dictionary<string, GameObject> tilePrefabDic = new Dictionary<string, GameObject> { };*/


    Creature currentCreature; // Cached here and used when spawing creatures into the level

    // List of tiles curently selected
    List<Tile> lightHighlightedTiles = new List<Tile> { };
    List<Tile> mediumHighlightedTiles = new List<Tile> { };
    List<Tile> heavyHighlightedTiles = new List<Tile> { };

    // Properties
    public int TileLeft
    {
        get { return tileLeft; }
    }
    public int TileRight
    {
        get { return tileRight; }
    }
    public int TileTop
    {
        get { return tileTop; }
    }
    public int TileBottom
    {
        get { return tileBottom; }
    }
    public float RealWidth
    {
        get { return realWidth; }
    }
    public float RealHeight
    {
        get { return realHeight; }
    }
    public float RealLeft
    {
        get { return realLeft; }
    }
    public float RealRight
    {
        get { return realRight; }
    }
    public float RealTop
    {
        get { return realTop; }
    }
    public float RealBottom
    {
        get { return realBottom; }
    }
    public Tile[,] Map
    {
        get { return map; }
    }

    // Start is called before the first frame update
    void Awake()
    {
        game = gameObject.GetComponent<Game>();

        prefabContainer = GameObject.FindGameObjectWithTag("prefabContainer").GetComponent<PrefabContainer>();

/*        // Create tile type dictionary
        foreach (GameObject tilePrefab in tilePrefabList)
        {
            tilePrefabDic.Add(tilePrefab.GetComponent<Tile>().DisplayName, tilePrefab);
        }

        // Create the obstacle dictionary
        foreach (GameObject obstaclePrefab in obstaclePrefabList)
        {
            obstaclePrefabDic.Add(obstaclePrefab.GetComponent<Obstacle>().DisplayName, obstaclePrefab);
        }*/
    }

    public virtual void SpawnLevel()
    {
        // Get the file from the main menu if its still there
        if (GameObject.FindGameObjectWithTag("Interscene") != null) // The main menu object was found
        {
            Debug.Log("Interscene manager found");
            mapFile = GameObject.FindGameObjectWithTag("Interscene").GetComponent<IntersceneManager>().LevelFile;
        }
        else // No main menu object was found
        {
            Debug.Log("Interscene manager not found");
        }

        // Open the file reader
        FileInfo sourceFile = new FileInfo(mapFile);
        StreamReader reader = sourceFile.OpenText();

        // The first line always says the map size in "X/Y" such as "10/10" or "50/20"
        string[] unParsedMapSize = reader.ReadLine().Split("/");
        tileWidth = int.Parse(unParsedMapSize[0]);
        tileHeight = int.Parse(unParsedMapSize[1]);

        map = new Tile[tileWidth, tileHeight];

        // Save the map of tile heights
        string[,] heightMapInFile = new string[tileWidth, tileHeight];
        for (int y = 0; y < tileHeight; y++)
        {
            string[] currentLine = reader.ReadLine().Split(",");
            for (int x = 0; x < tileWidth; x++)
            {
                heightMapInFile[x, y] = currentLine[x];
                //Debug.Log(string.Format("Saving tileMapInFile ({0},{1}) as {2}", x, y, tileMapInFile[x,y]));
            }
        }

        // There is a blank line between the 3 maps
        reader.ReadLine();

        // Save the map of tile heights
        string[,] tileTypeMapInFile = new string[tileWidth, tileHeight];
        for (int y = 0; y < tileHeight; y++)
        {
            string[] currentLine = reader.ReadLine().Split(",");
            for (int x = 0; x < tileWidth; x++)
            {
                tileTypeMapInFile[x, y] = currentLine[x];
                //Debug.Log(string.Format("Saving tileMapInFile ({0},{1}) as {2}", x, y, tileMapInFile[x,y]));
            }
        }

        // There is a blank line between the 3 maps
        reader.ReadLine();

        // Save the detail map
        string[,] detailMapInFile = new string[tileWidth, tileHeight];
        for (int y = 0; y < tileHeight; y++)
        {
            string[] currentLine = reader.ReadLine().Split(",");
            for (int x = 0; x < tileWidth; x++)
            {
                detailMapInFile[x, y] = currentLine[x];
                //Debug.Log(string.Format("Saving detailMapInFile ({0},{1}) as {2}", x, y, detailMapInFile[x, y]));
            }
        }

        reader.Close();

        int playerIndex = 0;

        // Create the actual grid of tiles
        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                // Create the tile at the propper height (don't create() until we also make the detail)

                // Store the height value for this tile
                float currentTileHeight = float.Parse(heightMapInFile[x, y]);
                string currentTileType = tileTypeMapInFile[x, y];

                // Create the tile of the proper type
                if (prefabContainer.TilePrefabs.ContainsKey(currentTileType)) // This tile type exists
                {
                    // Create the tile out of the correct prefab (based on the tile type) at the height we found
                    map[x, y] = Instantiate(prefabContainer.TilePrefabs[currentTileType], new Vector3(x * tileSize, currentTileHeight, y * tileSize), Quaternion.identity).GetComponent<Tile>();
                }
                else // This tile type does not exist
                {
                    // Default to the first tile type
                    Debug.LogError("Unrecognized tile type: \"" + currentTileType + "\"");
                    map[x, y] = Instantiate(prefabContainer._tilePrefabList[0], new Vector3(x * tileSize, currentTileHeight, y * tileSize), Quaternion.identity).GetComponent<Tile>();
                }

                // Determine which direction to randomly rotate this spawned object if there is one
                float randomRotation;

                // Put down different things depending on what the detail map says
                // TODO: Make player and enemey spawns put them into the respective team managers
                switch (detailMapInFile[x,y])
                {
                    case "_": // Blank
                        map[x, y].Create(x, y, currentTileHeight);
                        break;

                    case "PlayerSpawn": // Player spawn
                        randomRotation = Random.Range(0, 360);
                        // TODO: Use the player dictionary probably
                        // TODO: Feed the player the data they should load with
                        currentCreature = Instantiate(prefabContainer._playerPrefabList[playerIndex], new Vector3 (x * tileSize, currentTileHeight, y * tileSize), Quaternion.Euler(0, randomRotation, 0)).GetComponent<Player>();
                        map[x, y].Create(x, y, currentTileHeight, currentCreature);
                        game.NewCreature(currentCreature);

                        // Incriment the player index
                        playerIndex += 1;
                        if (playerIndex >= prefabContainer._playerPrefabList.Count) // The index has grown too much
                        {
                            // Reset the index
                            Debug.LogError("Too many player spawns");
                            playerIndex = 0;
                        }
                        break;

                    case "EnemySpawn": // Enemy Spawn
                        randomRotation = Random.Range(0, 360);
                        // TODO: Use the enemy dictionary when multiple enemy types are added instead of always using index 0
                        currentCreature = Instantiate(prefabContainer._enemyPrefabList[0], new Vector3(x * tileSize, currentTileHeight, y * tileSize), Quaternion.Euler(0, randomRotation, 0)).GetComponent<Creature>();
                        map[x, y].Create(x, y, currentTileHeight, currentCreature);
                        game.NewCreature(currentCreature);
                        break;

                    default: // Obstacle
                        // Check if its a recognized obstacle
                        string currentDetail = detailMapInFile[x, y];
                        if (prefabContainer.ObstaclePrefabs.ContainsKey(currentDetail)) // It is an obstacle
                        {
                            randomRotation = Random.Range(0, 360);
                            Obstacle currentObstacle = Instantiate(prefabContainer.ObstaclePrefabs[currentDetail], new Vector3(x * tileSize, currentTileHeight, y * tileSize), Quaternion.Euler(0, randomRotation, 0)).GetComponent<Obstacle>();
                            map[x, y].Create(x, y, currentTileHeight, currentObstacle);
                        }
                        else // It is unrecognized
                        {
                            // Treat it as blank
                            Debug.LogError("Unrecognized detail: \"" + currentDetail + "\"");
                            map[x, y].Create(x, y, currentTileHeight);
                        }
                        break;
                }
            }
        }

        CalculateMapEdges();

        // Find each tile its connections
        // TODO: Update each tile's connections list whenever an adjasent tile is created rather than all at the end
        // TODO: Store map data for different connection distances (rather than them all being 1)
        foreach (Tile tile in map)
        {
            tile.CalculateConnections();
        }

        // The level is now fully spawned
        //game.LevelComplete();
    }

    protected virtual void CalculateMapEdges()
    {
        // Calculate the edges
        tileLeft = 0;
        tileRight = tileWidth - 1;
        tileBottom = 0;
        tileTop = tileWidth - 1;

        // Calculate the real world size
        realWidth = tileWidth * tileSize;
        realHeight = tileHeight * tileSize;
        realLeft = tileSize * (-0.5f);
        realRight = realLeft + realWidth;
        realBottom = tileSize * (-0.5f);
        realTop = realBottom + realHeight;
    }

    public Tile TargetTile(Vector3 pointerPosition)
    {
        // Make sure its in bounds
        if (RealPointOnMap(pointerPosition)) // It is in bounds
        {
            int x = (int)(Mathf.Floor(pointerPosition.x + tileSize / 2) / tileSize);
            int y = (int)(Mathf.Floor(pointerPosition.z + tileSize / 2) / tileSize);
            return map[x,y];
        }
        else // Its out of bounds
        {
            // Debug.Log("Target Tile Null");
            return null;
        }
    }
    public Tile TargetTile(Vector2 pointerPosition)
    {
        return TargetTile(new Vector3(pointerPosition.x, 0, pointerPosition.y));
    }
    
    public Tile TargetTile(int x, int y)
    {
        // Debug.Log(string.Format("Left: {0}\nRight: {1}\nBottom: {2}\nTop: {3}\nX: {4}\nY: {5}", tileLeft, tileRight, tileBottom, tileTop, x, y));
        // Make sure its in bounds
        if (TilePointOnMap(x, y))
        {
            return map[x, y];
        }
        else // Its not in range
        {
            Debug.Log("Target Tile Null");
            return null;
        }
    }

    public bool RealPointOnMap(Vector2 pos)
    {
        return (
            pos.x > realLeft &&
            pos.x < realRight &&
            pos.y > realBottom &&
            pos.y < realTop);
    }
    public bool RealPointOnMap(float x, float z)
    {
        return RealPointOnMap(new Vector2(x, z));
    }
    public bool RealPointOnMap(Vector3 pos)
    {
        return RealPointOnMap(new Vector2(pos.x, pos.z));
    }

    public bool TilePointOnMap(int x, int y)
    {
        return (
            x >= tileLeft &&
            x <= tileRight &&
            y >= tileBottom &&
            y <= tileTop);
    }
    public bool TilePointOnMap(float x, float y)
    {
        return TilePointOnMap((int)x, (int)y);
    }
    public bool TilePointOnMap(Vector2 pos)
    {
        return TilePointOnMap(pos.x, pos.y);
    }

    protected float GetDistSqr(Tile tile1, Tile tile2)
    {
        return Mathf.Pow(tile1.x - tile2.x, 2) + Mathf.Pow(tile1.y - tile2.y, 2);
    }
    protected float GetDistSqr(Tile tile1, int x2, int y2)
    {
        return Mathf.Pow(tile1.x - x2, 2) + Mathf.Pow(tile1.y - y2, 2);
    }

    public bool WithinDistance(Tile tile1, Tile tile2, float distance)
    {
        return (GetDistSqr(tile1, tile2) <= Mathf.Pow(distance, 2));
    }
    public bool WithinDistance(Creature creature1, Creature creature2, float distance)
    {
        return WithinDistance(creature1.Space, creature2.Space, distance);
    }

    public Creature NearestCreature(List<Creature> creatures, Tile origin)
    {
        // Default to the first creature in the index
        Creature nearestCreature = creatures[0];
        float curentDistanceSqr = GetDistSqr(creatures[0].Space, origin);
        float shortestDistanceSqr = curentDistanceSqr;

        // Loop through each creature in the list and mark the one thats the closest
        for (int i = 1; i < creatures.Count; i ++)
        {
            // Calculate the distance
            curentDistanceSqr = GetDistSqr(creatures[i].Space, origin);

            // Test if its closer than the previously recorded closest
            if (curentDistanceSqr < shortestDistanceSqr) // This one is closer
            {
                shortestDistanceSqr = curentDistanceSqr;
                nearestCreature = creatures[i];
            }
        }

        return nearestCreature;
    }
    public Creature NearestCreature(List<Creature> creatures, Creature origin)
    {
        return NearestCreature(creatures, origin.Space);
    }

    public bool NearestCreatureWithinDistance(List<Creature> creatures, Creature origin, float distance)
    {
        return WithinDistance(NearestCreature(creatures, origin), origin, distance);
    }

    protected bool OnTopSide(Vector2 startPosition, Vector2 endPosition, Vector2 checkPosition)
    {
        return (endPosition.x - startPosition.x) * (checkPosition.y - startPosition.y) - (endPosition.y - startPosition.y) * (checkPosition.x - startPosition.x) > 0 ;

    }
    protected bool OnTopSide(Tile startTile, Tile targetTile, int checkX, int checkY)
    {
        return OnTopSide(startTile.TilePosition, targetTile.TilePosition, new Vector2(checkX, checkY));
    }
    protected bool OnTopSide(Tile startTile, Vector2 endPosition, int checkX, int checkY)
    {
        return OnTopSide(startTile.TilePosition, endPosition, new Vector2(checkX, checkY));
    }

    protected bool OnBottomSide(Vector2 startPosition, Vector2 endPosition, Vector2 checkPosition)
    {
        return (endPosition.x - startPosition.x) * (checkPosition.y - startPosition.y) - (endPosition.y - startPosition.y) * (checkPosition.x - startPosition.x) < 0;

    }
    protected bool OnBottomSide(Tile startTile, Tile targetTile, int checkX, int checkY)
    {
        return OnBottomSide(startTile.TilePosition, targetTile.TilePosition, new Vector2(checkX, checkY));
    }
    protected bool OnBottomSide(Tile startTile, Vector2 endPosition, int checkX, int checkY)
    {
        return OnBottomSide(startTile.TilePosition, endPosition, new Vector2(checkX, checkY));
    }

    public List<Tile> AdjacentTiles(Tile centerTile)
    {
        // Create an empty list and fill it with valid tiles
        
        List<Tile> validTiles = new List<Tile> { };

        // Loop through all spaces that should be adjasent and make sure they're on the map
        for (int x = centerTile.x - 1; x <= centerTile.x + 1; x ++)
        {
            for (int y = centerTile.y - 1; y <= centerTile.y + 1; y++)
            {
                if (x >= 0 && // It is not off the -x side
                    x <= tileWidth - 1 && // It is not off the +x side
                    y >= 0 && // It is not off the -y side
                    y <= tileHeight - 1 // It is not off the +y side
                )
                {
                    // Add the tile to the list
                    validTiles.Add(map[x, y]);
                }
            }
        }

        return validTiles;
    }

    public bool IsTileAdjacent(Tile startTile, Tile targetTile)
    {
        // Return false if the dif in X or Y is greater than 1
        return (Mathf.Abs(startTile.x - targetTile.x) <= 1 && Mathf.Abs(startTile.x - targetTile.x) <= 1);
    }

    public List<Tile> TilesInRange(Tile centerTile, float range, float startingHeight, float heightMod, bool rangeCapped)
    {
        List<Tile> tiles = new List<Tile> { };
        float rangeSqr = Mathf.Pow(range, 2);

        // Loop through every tile and add it to the list if its within range
        // TODO: Only check the tiles that are within X dif or y dif = range rather than every single tile
        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                // TODO: Round down on distance to not need to add 0.5 to all ranges for attacks.
                // Might require not using squared distance, which would be very slow

                // Find the height diff to add to the range
                // TODO: Maybe use tile.TopHeight instead of tile.Height (though it breaks things rn)
                float heightDiff = (centerTile.Height + startingHeight) - Map[x, y].TargetHeight;

                float tempRangeSqr = 0;

                // Only calculate a new range squared if the source is looking down (maybe make this toggleable as a boolean)
                if (heightDiff > 0) // The source is looking down
                {
                    // Calculate a new squared range
                    tempRangeSqr = Mathf.Pow(range + (heightDiff * heightMod), 2);
                }
                else // The source is looking forward or up
                {
                    // use the already calculated range sqr
                    tempRangeSqr = rangeSqr;
                }

                // Test if its in range
                float distSqr = GetDistSqr(centerTile, x, y);
                // Add the height difference if the range should be capped
                if (rangeCapped) // It is a melee attack or something and should not be able to go past a certain range, reguardless of height difference
                {
                    // The height difference should count as part of the range (reaching down too far with a sword makes it harder to hit)
                    distSqr += Mathf.Pow(heightDiff, 2);
                }
                if (distSqr <= tempRangeSqr) // It is in range
                {
                    // Add this tile to the list
                    tiles.Add(map[x, y]);                    
                }
            }
        }

        return tiles;
    }
    public List<Tile> TilesInRange(Creature creature, float range, float heightMod, bool rangeCapped)
    {
        return TilesInRange(creature.Space, range, creature.EyeHeight, heightMod, rangeCapped);
    }
    public List<Tile> TilesInRange(int x, int y, float range, float startingHeight, float heightMod, bool rangeCapped)
    {
        Tile centerTile = map[x, y];

        return TilesInRange(centerTile, range, startingHeight, heightMod, rangeCapped);
    }

    public List<Tile> TilesInLine(Tile startTile, Tile targetTile, float length, float width)
    {
        
        List<Tile> tiles = new List<Tile> { };
        float lengthSqr = Mathf.Pow(length, 2);
        float radius = width / 2;

        // Get a perpendicular line
        Vector2 perpLinePoint = new Vector2(startTile.x - (startTile.y - targetTile.y), startTile.y + (startTile.x - targetTile.x));

        // Loop through every tile and add it to the list if its within range
        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                // First check if the tile is within range to be in the line at all
                if (GetDistSqr(startTile, x, y) <= lengthSqr)
                {
                    // Check if its on the right side
                    if (OnTopSide(startTile, perpLinePoint, x, y))
                    //if ((perpLinePoint.x - startTile.x) * (y - startTile.y) - (perpLinePoint.y - startTile.y) * (x - startTile.x) > 0) // Its on the right side
                    {
                        // Only do this once you know its on the right side to cut down on calculations
                        // Long complicated math from this video: https://www.youtube.com/watch?v=KHuI9bXZS74
                        // Calculate distance from line
                        float distance = Mathf.Abs((x - startTile.x) * (-targetTile.y + startTile.y) + (y - startTile.y) * (targetTile.x - startTile.x)) /
                            Mathf.Sqrt(Mathf.Pow(-targetTile.y + startTile.y, 2) + Mathf.Pow(targetTile.x - startTile.x, 2));

                        if (distance <= radius) // Its in range
                        {
                            tiles.Add(map[x, y]);
                        }
                    }

                }
            }
        }

        return tiles;
    }
    public List<Tile> TilesInLine(Tile startTile, Tile targetTile, float width)
    {
        
        List<Tile> tiles = new List<Tile> { };
        float lengthSqr = GetDistSqr(startTile, targetTile);
        float radius = width / 2;

        // Get a perpendicular line
        Vector2 perpLinePoint = new Vector2(startTile.x - (startTile.y - targetTile.y), startTile.y + (startTile.x - targetTile.x));

        // Loop through every tile and add it to the list if its within range
        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                // First check if the tile is within range to be in the line at all
                if (GetDistSqr(startTile, x, y) <= lengthSqr)
                {
                    // Check if its on the right side
                    if (OnTopSide(startTile, perpLinePoint, x, y))
                    //if ((perpLinePoint.x - startTile.x) * (y - startTile.y) - (perpLinePoint.y - startTile.y) * (x - startTile.x) > 0) // Its on the right side
                    {
                        // Only do this once you know its on the right side to cut down on calculations
                        // Long complicated math from this video: https://www.youtube.com/watch?v=KHuI9bXZS74
                        // Calculate distance from line
                        float distance = Mathf.Abs((x - startTile.x) * (-targetTile.y + startTile.y) + (y - startTile.y) * (targetTile.x - startTile.x)) /
                            Mathf.Sqrt(Mathf.Pow(-targetTile.y + startTile.y, 2) + Mathf.Pow(targetTile.x - startTile.x, 2));

                        if (distance <= radius) // Its in range
                        {
                            tiles.Add(map[x, y]);
                        }
                    }

                }
            }
        }

        return tiles;
    }

    public List<Tile> TilesInCone(Tile startTile, Tile targetTile, float length, float angle)
    {
        
        List<Tile> tiles = new List<Tile> { };
        float lengthSqr = Mathf.Pow(length, 2);
        float angleInRads = angle * Mathf.Deg2Rad;
        float angleFromCenter = angleInRads / 2;
        float closeValue = 2;

        // Calculate the new edge lines
        Vector2 centerLine = new Vector2(targetTile.x - startTile.x, targetTile.y - startTile.y);
        float originalAngle = Mathf.Asin(centerLine.y / centerLine.magnitude);
        float originalAngleCos = Mathf.Acos(centerLine.x / centerLine.magnitude);
        // Make sure its in the right quadrent
        if (originalAngleCos >= Mathf.PI/2) // Its on the left side (where Asin is flipped)
        {
            originalAngle = Mathf.PI - originalAngle;
        }
        float topAngle = originalAngle + angleFromCenter;
        float bottomAngle = originalAngle - angleFromCenter;
        Vector2 topEdgePoint = new Vector2(Mathf.Cos(topAngle)* closeValue + startTile.x, Mathf.Sin(topAngle)* closeValue + startTile.y);
        Vector2 bottomEdgePoint = new Vector2(Mathf.Cos(bottomAngle)* closeValue + startTile.x, Mathf.Sin(bottomAngle)* closeValue + startTile.y);

        //List<Tile> tempList = new List<Tile> { };
        //Tile topEdgeTile = TargetTile(topEdgePoint);
        //Tile bottomEdgeTile = TargetTile(bottomEdgePoint);
        //if (topEdgeTile != null && bottomEdgeTile != null)
        //{
        //    tempList = TilesInLine(startTile, topEdgeTile, length, 1);
        //    foreach(Tile tile in tempList)
        //    {
        //        tiles.Add(tile);
        //    }
        //    tempList = TilesInLine(startTile, bottomEdgeTile, length, 1);
        //    foreach (Tile tile in tempList)
        //    {
        //        tiles.Add(tile);
        //    }
        //}

        // Loop through every tile and add it to the list if its within range
        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                // First check if the tile is within range to be in the line at all
                if (GetDistSqr(startTile, x, y) <= lengthSqr)
                {
                    // Check if its bellow the top
                    if (OnBottomSide(startTile, topEdgePoint, x, y))
                    {
                        // Check kif its above the bottom (only do this if its bellow the top to cut down on computation time
                        if (OnTopSide(startTile, bottomEdgePoint, x, y))
                        {
                            tiles.Add(map[x, y]);
                        }
                    }
                }
            }
        }

        return tiles;
    }

    public List<Creature> CreaturesInList(List<Tile> tiles)
    {
        // Create an empty list
        List<Creature> creatures = new List<Creature> { };

        // Loop through each tile in the list
        foreach(Tile tile in tiles)
        {
            // Check if it has an occupant
            if (tile.HasOccupant) // The tile has an occupant
            {
                creatures.Add(tile.Occupant);
            }
        }

        return creatures;
    }

    public List<Tile> TilesWithCreatures(List<Tile> tiles)
    {
        // Create an empty list
        List<Tile> tilesWithCreatures = new List<Tile> { };

        // Loop through each tile in the list
        foreach (Tile tile in tiles)
        {
            // Check if it has an occupant
            if (tile.HasOccupant) // The tile has an occupant
            {
                tilesWithCreatures.Add(tile);
            }
        }

        return tilesWithCreatures;
    }

    public List<Tile> ListOfSpaces(List<Creature> creatures)
    {
        // Create an empty list
        List<Tile> spaces = new List<Tile> { };

        // Loop through each creature in the list
        foreach (Creature creature in creatures)
        {
            spaces.Add(creature.Space);
        }

        return spaces;
    }

    public bool HasLineOfSight(Tile subjectTile, float subjectEyeHeight, Tile targetTile, float targetHeight, bool ignoreCreatures)
    {
        // Get a list of all tiles between the subject and target, not including themselves
        List<Tile> tilesInBetween = TilesInLine(subjectTile, targetTile, 1.0f);
        tilesInBetween.Remove(subjectTile);
        tilesInBetween.Remove(targetTile);

        // TODO: removed targetHeight variable from targetWorldHeight. Maybe remove it as a parameter?

        float subjectWorldHeight = subjectTile.Height + subjectEyeHeight; // Height of the top of the subject's head
        float targetWorldHeight = targetTile.TargetHeight; // Height of the top of the target's head
        float angle = Mathf.Atan((targetWorldHeight - subjectWorldHeight) / Mathf.Sqrt(GetDistSqr(subjectTile, targetTile))); // Angle between the subject's height and the target's
        
        // Check if each tile is tall enough to block line of sight
        foreach (Tile tile in tilesInBetween)
        {

            // Calculate the height
            float heightDif = tile.BlockHeight - subjectWorldHeight;

            // Comment out if creatures shouldn't block line of sight and ignoreCreautres is false
            if (tile.HasOccupant && !ignoreCreatures) // Has a creature in it
            {
                heightDif += tile.Occupant.Height;
            }

            // Calculate the distance
            float distance = Mathf.Sqrt(GetDistSqr(subjectTile, tile));

            // Check if the tile is too tall
            float acceptableHeightDif = Mathf.Sin(angle) * distance;
            if (heightDif > acceptableHeightDif) // It is too tall
            {
                return false;
            }
        }

        // If we got here without returning false once then they do have line of sight
        return true;
    }
    public bool HasLineOfSight(Creature subject, Creature target, bool ignoreCreatures)
    {
        return HasLineOfSight(subject.Space, subject.EyeHeight, target.Space, target.Height, ignoreCreatures);
    }
    public bool HasLineOfSight(Creature subject, Tile target, bool ignoreCreatures)
    {
        // Fill in 0 for the height of the target since its empty
        return HasLineOfSight(subject.Space, subject.EyeHeight, target, 0, ignoreCreatures);
    }
    public bool HasLineOfSight(Tile subjectTile, float subjectEyeHeight, Tile targetTile, bool ignoreCreatures)
    {
        // Use the height of the creature in the target if there is one. Otherwise, just use 0
        if (targetTile.HasOccupant) // There is a creature in the target tile
        {
            return HasLineOfSight(subjectTile, subjectEyeHeight, targetTile, targetTile.Occupant.Height, ignoreCreatures);
        }
        else // There is not a creature in the target tile
        {
            return HasLineOfSight(subjectTile, subjectEyeHeight, targetTile, 0, ignoreCreatures);
        }
    }

    // Create a new list with only the tiles with line of sight in it
    public List<Tile> LineOfSight(List<Tile> tiles, Tile origin, float originHeight, bool ignoreCreatures)
    {
        // Create a new empty list
        
        List<Tile> newList = new List<Tile> { };

        // Add each tile if it has line of sight
        foreach (Tile tile in tiles)
        {
            if (HasLineOfSight(origin, originHeight, tile, ignoreCreatures) && !tile.HasObstacle) // It does have line of sight and is open
            {
                newList.Add(tile);
                tilesBeingChecked.Add(new checkedTiles(origin.RealPosition, tile.RealPosition, true));
            }
            else
            {
                tilesBeingChecked.Add(new checkedTiles(origin.RealPosition, tile.RealPosition, false));
            }
        }

        // Return the now complete list
        return newList;
    }
    public List<Tile> LineOfSight(List<Tile> tiles, Creature subject, bool ignoreCreatures)
    {
        return LineOfSight(tiles, subject.Space, subject.EyeHeight, ignoreCreatures);
    }
    public List<Tile> LineOfSight(List<Tile> tiles, Tile origin, bool ignoreCreatures)
    {
        return LineOfSight(tiles, origin, 0, ignoreCreatures);
    }

    // Highlight light, medium, and heavy tiles
    public void HighlightTiles(List<Tile> lightTiles, List<Tile> mediumTiles, List<Tile> heavyTiles)
    {
        // Note: Some tiles may be in multiple of these lists. The heavier level trumps the ones before it

        // Unhighlight all the old tiles
        UnHighlightAllTiles();

        // Highlight the light tiles
        foreach (Tile tile in lightTiles)
        {
            // Highlight the tile
            tile.Highligted = highlightLevels.light;

            // Mark that this is a curently highlighted tile
            lightHighlightedTiles.Add(tile);
            // TODO: This will cause some tiles to appear on the list multiple times, which will make UnhighlightTiles() longer
        }

        // Highlight the medium tiles
        foreach (Tile tile in mediumTiles)
        {
            // Highlight the tile
            tile.Highligted = highlightLevels.medium;

            // Mark that this is a curently highlighted tile
            mediumHighlightedTiles.Add(tile);
        }

        // Highlight the heavy tiles
        foreach (Tile tile in heavyTiles)
        {
            // Highlight the tile
            tile.Highligted = highlightLevels.heavy;

            // Mark that this is a curently highlighted tile
            heavyHighlightedTiles.Add(tile);
        }
    }
    // Highlight only the medium and light tiles
    public void HighlightTiles(List<Tile> lightTiles, List<Tile> mediumTiles)
    {
        // Note: Some tiles may be in multiple of these lists. The heavier level trumps the ones before it

        // Unhighlight the old tiles
        UnHighlightLightTiles();
        UnHighlightMediumTiles();

        // Highlight the light tiles
        foreach (Tile tile in lightTiles)
        {
            // Highlight the tile
            tile.Highligted = highlightLevels.light;

            // Mark that this is a curently highlighted tile
            lightHighlightedTiles.Add(tile);
            // TODO: This will cause some tiles to appear on the list multiple times, which will make UnhighlightTiles() longer
        }

        // Highlight the medium tiles
        foreach (Tile tile in mediumTiles)
        {
            // Highlight the tile
            tile.Highligted = highlightLevels.medium;

            // Mark that this is a curently highlighted tile
            mediumHighlightedTiles.Add(tile);
        }
    }
    // Highlight only the heavy tiles
    public void HighlightTiles(List<Tile> heavyTiles)
    {
        UnHighlightHeavyTiles();

        // Highlight the light tiles
        foreach (Tile tile in heavyTiles)
        {
            // Highlight the tile
            tile.Highligted = highlightLevels.heavy;

            // Mark that this is a curently highlighted tile
            heavyHighlightedTiles.Add(tile);
        }
    }
    public void UnHighlightLightTiles()
    {
        // Unhighlight each light tile
        foreach (Tile tile in lightHighlightedTiles)
        {
            tile.Highligted = highlightLevels.none;
        }

        // Clear the list
        lightHighlightedTiles.Clear();
    }
    public void UnHighlightMediumTiles()
    {
        // Unhighlight each medium tile
        foreach (Tile tile in mediumHighlightedTiles)
        {
            if (lightHighlightedTiles.Contains(tile))
            {
                tile.Highligted = highlightLevels.light;
            }
            else
            {
                tile.Highligted = highlightLevels.none;
            }
        }

        // Clear the list
        mediumHighlightedTiles.Clear();
    }
    public void UnHighlightHeavyTiles()
    {
        // Unhighlight each heavy tile
        foreach (Tile tile in heavyHighlightedTiles)
        {
            if (mediumHighlightedTiles.Contains(tile))
            {
                tile.Highligted = highlightLevels.medium;
            }
            else if (lightHighlightedTiles.Contains(tile))
            {
                tile.Highligted = highlightLevels.light;
            }
            else
            {
                tile.Highligted = highlightLevels.none;
            }
        }

        // Clear the list
        heavyHighlightedTiles.Clear();
    }
    public void UnHighlightAllTiles()
    {
        // Unhighlight all tiles
        UnHighlightLightTiles();
        UnHighlightMediumTiles();
        UnHighlightHeavyTiles();
    }
}
