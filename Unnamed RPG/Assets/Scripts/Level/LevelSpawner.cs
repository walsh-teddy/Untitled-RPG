using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LevelSpawner : MonoBehaviour
{
    // Variables
    int tileWidth = 10;
    int tileHeight = 10;
    [SerializeField] string mapFile;
    public float tileSize = 1;
    private int tileLeft, tileRight, tileTop, tileBottom;
    private float realWidth, realHeight;
    private float realLeft, realRight, realTop, realBottom;
    private Tile[,] map;
    private Game game;
    private PlayerManager playerManager;

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
    private void OnDrawGizmos()
    {
        foreach (checkedTiles checkedTile in tilesBeingChecked)
        {
            if (checkedTile.valid)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawLine(checkedTile.source, checkedTile.target);
        }

        // Clear out the checked tiles
        tilesBeingChecked.Clear();
    }

    // Objects to be created or moved around
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPreafab;
    [SerializeField] private GameObject testPlayerPreafab; // TODO: Remove this
    [SerializeField] private GameObject enemyPreafab;
    [SerializeField] private GameObject grassTilePreafab;
    [SerializeField] private GameObject rockTilePreafab;
    [SerializeField] private GameObject rockPreafab;

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

    // Start is called before the first frame update
    void Awake()
    {
        game = gameObject.GetComponent<Game>();
        playerManager = gameObject.GetComponent<PlayerManager>();
    }

    public void SpawnLevel()
    {
        // Open the file reader
        FileInfo sourceFile = new FileInfo(mapFile);
        StreamReader reader = sourceFile.OpenText();

        // The first line always says the map size in "X/Y" such as "10/10" or "50/20"
        string[] unParsedMapSize = reader.ReadLine().Split("/");
        tileWidth = int.Parse(unParsedMapSize[0]);
        tileHeight = int.Parse(unParsedMapSize[1]);

        map = new Tile[tileWidth, tileHeight];

        // Save the map of tile heights
        string[,] tileMapInFile = new string[tileWidth, tileHeight];
        for (int y = 0; y < tileHeight; y++)
        {
            string[] currentLine = reader.ReadLine().Split(",");
            for (int x = 0; x < tileWidth; x++)
            {
                tileMapInFile[x, y] = currentLine[x];
                //Debug.Log(string.Format("Saving tileMapInFile ({0},{1}) as {2}", x, y, tileMapInFile[x,y]));
            }
        }

        // There is a blank line between the 2 maps
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

        // Create the actual grid of tiles
        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                // Create the tile at the propper height (don't create() until we also make the detail)
                int currentTileHeight = int.Parse(tileMapInFile[x, y]);
                map[x, y] = Instantiate(grassTilePreafab, new Vector3(x * tileSize, currentTileHeight, y * tileSize), Quaternion.identity).GetComponent<Tile>();

                // Determine which direction to randomly rotate this spawned object if there is one
                float randomRotation;

                // Put down different things depending on what the detail map says
                switch (detailMapInFile[x,y])
                {
                    case "_": // Blank
                        map[x, y].Create(x, y, currentTileHeight);
                        break;

                    case "P": // Player Spawn
                        randomRotation = Random.Range(0, 360);
                        Player currentPlayer = Instantiate(playerPreafab, new Vector3 (x * tileSize, currentTileHeight, y * tileSize), Quaternion.Euler(0, randomRotation, 0)).GetComponent<Player>();
                        map[x, y].Create(x, y, currentTileHeight, currentPlayer);
                        game.NewCreature(currentPlayer);
                        break;

                    case "T": // Test Player Spawn (TODO: Remove this)
                        randomRotation = Random.Range(0, 360);
                        Player currentTestPlayer = Instantiate(testPlayerPreafab, new Vector3(x * tileSize, currentTileHeight, y * tileSize), Quaternion.Euler(0, randomRotation, 0)).GetComponent<Player>();
                        map[x, y].Create(x, y, currentTileHeight, currentTestPlayer);
                        game.NewCreature(currentTestPlayer);
                        break;

                    case "R": // Rock Obstacle
                        randomRotation = Random.Range(0, 360);
                        Obstacle currentRock = Instantiate(rockPreafab, new Vector3(x * tileSize, currentTileHeight, y * tileSize), Quaternion.Euler(0, randomRotation, 0)).GetComponent<Obstacle>();
                        map[x, y].Create(x, y, currentTileHeight, currentRock);
                        break;

                    case "E": // Enemy Spawn
                        randomRotation = Random.Range(0, 360);
                        Creature currentEnemy = Instantiate(enemyPreafab, new Vector3(x * tileSize, currentTileHeight, y * tileSize), Quaternion.Euler(0, randomRotation, 0)).GetComponent<Creature>();
                        map[x, y].Create(x, y, currentTileHeight, currentEnemy);
                        game.NewCreature(currentEnemy);
                        break;

                    default: // Something went wrong
                        Debug.Log(string.Format("Error at ({0},{1})", x, y));
                        break;
                }
            }
        }

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

        // The level is now fully spawned
        game.LevelComplete();
    }

    public Tile TargetTile(Vector3 pointerPosition)
    {
        // Make sure its in bounds
        if (RealPointOnMap(pointerPosition)) // It is in bounds
        {
            Vector2 position2D = new Vector2(Mathf.Floor(pointerPosition.x + tileSize / 2), Mathf.Floor(pointerPosition.z + tileSize / 2))/tileSize;
            return map[(int)position2D.x, (int)position2D.y];
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

    private float GetDistSqr(Tile tile1, Tile tile2)
    {
        return Mathf.Pow(tile1.x - tile2.x, 2) + Mathf.Pow(tile1.y - tile2.y, 2);
    }
    private float GetDistSqr(Tile tile1, int x2, int y2)
    {
        return Mathf.Pow(tile1.x - x2, 2) + Mathf.Pow(tile1.y - y2, 2);
    }

    private bool OnTopSide(Vector2 startPosition, Vector2 endPosition, Vector2 checkPosition)
    {
        return (endPosition.x - startPosition.x) * (checkPosition.y - startPosition.y) - (endPosition.y - startPosition.y) * (checkPosition.x - startPosition.x) > 0 ;

    }
    private bool OnTopSide(Tile startTile, Tile targetTile, int checkX, int checkY)
    {
        return OnTopSide(startTile.TilePosition, targetTile.TilePosition, new Vector2(checkX, checkY));
    }
    private bool OnTopSide(Tile startTile, Vector2 endPosition, int checkX, int checkY)
    {
        return OnTopSide(startTile.TilePosition, endPosition, new Vector2(checkX, checkY));
    }

    private bool OnBottomSide(Vector2 startPosition, Vector2 endPosition, Vector2 checkPosition)
    {
        return (endPosition.x - startPosition.x) * (checkPosition.y - startPosition.y) - (endPosition.y - startPosition.y) * (checkPosition.x - startPosition.x) < 0;

    }
    private bool OnBottomSide(Tile startTile, Tile targetTile, int checkX, int checkY)
    {
        return OnBottomSide(startTile.TilePosition, targetTile.TilePosition, new Vector2(checkX, checkY));
    }
    private bool OnBottomSide(Tile startTile, Vector2 endPosition, int checkX, int checkY)
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

        // Load this in once rather than repeatedly
        Vector2 centerTilePos = centerTile.TilePosition;

        // Loop through every tile and add it to the list if its within range
        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                if (x >= centerTilePos.x - 1 &&
                    x <= centerTilePos.x + 1 &&
                    y >= centerTilePos.y - 1 &&
                    y <= centerTilePos.y + 1 && // Its in range
                    TilePointOnMap(centerTilePos) && // Its in bounds of the map
                    !(x == centerTilePos.x && y == centerTilePos.y)) // Its not the center tile
                {
                    validTiles.Add(map[x, y]);
                }

                Debug.Log("(" + x + "/" + y + ")" + (x >= centerTilePos.x - 1) + (x <= centerTilePos.x + 1) + (y >= centerTilePos.y - 1) + (y <= centerTilePos.y + 1) + (TilePointOnMap(centerTilePos)));
            }
        }
    }

    public bool IsTileAdjacent(Tile startTile, Tile targetTile)
    {
        // Return false if the dif in X or Y is greater than 1
        return (Mathf.Abs(startTile.x - targetTile.x) <= 1 && Mathf.Abs(startTile.x - targetTile.x) <= 1);
    }

    public List<Tile> TilesInRange(Tile centerTile, float range)
    {
        
        List<Tile> tiles = new List<Tile> { };
        float rangeSqr = Mathf.Pow(range, 2);

        // Loop through every tile and add it to the list if its within range
        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeight; y++)
            {
                if (GetDistSqr(centerTile, x, y) <= rangeSqr)
                {
                    tiles.Add(map[x, y]);
                }
            }
        }

        return tiles;
    }
    public List<Tile> TilesInRange(Creature creature, float range)
    {
        return TilesInRange(creature.Space, range);
    }
    public List<Tile> TilesInRange(int x, int y, float range)
    {
        Tile centerTile = map[x, y];

        return TilesInRange(centerTile, range);
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

        // Loop through each tile in the list
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

        float subjectWorldHeight = subjectTile.Height + subjectEyeHeight; // Height of the top of the subject's head
        float targetWorldHeight = targetTile.Height + targetHeight; // Height of the top of the target's head
        float angle = Mathf.Atan((targetWorldHeight - subjectWorldHeight) / Mathf.Sqrt(GetDistSqr(subjectTile, targetTile))); // Angle between the subject's height and the target's
        
        // Check if each tile is tall enough to block line of sight
        foreach (Tile tile in tilesInBetween)
        {

            // Calculate the height
            float heightDif = tile.Height - subjectWorldHeight;

            // Add the height of any occupant if there is one
            if (tile.HasObstacle) // Has an obstacle
            {
                heightDif += tile.Obstacle.Height;
            }
            // Comment out if creatures shouldn't block line of sight and ignoreCreautres is false
            else if (tile.HasOccupant && !ignoreCreatures) // Has a creature in it
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
                tilesBeingChecked.Add(new checkedTiles(origin.realPosition, tile.realPosition, true));
            }
            else
            {
                tilesBeingChecked.Add(new checkedTiles(origin.realPosition, tile.realPosition, false));
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
            tile.Highligted = Tile.highlightLevels.light;

            // Mark that this is a curently highlighted tile
            lightHighlightedTiles.Add(tile);
            // TODO: This will cause some tiles to appear on the list multiple times, which will make UnhighlightTiles() longer
        }

        // Highlight the medium tiles
        foreach (Tile tile in mediumTiles)
        {
            // Highlight the tile
            tile.Highligted = Tile.highlightLevels.medium;

            // Mark that this is a curently highlighted tile
            mediumHighlightedTiles.Add(tile);
        }

        // Highlight the heavy tiles
        foreach (Tile tile in heavyTiles)
        {
            // Highlight the tile
            tile.Highligted = Tile.highlightLevels.heavy;

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
            tile.Highligted = Tile.highlightLevels.light;

            // Mark that this is a curently highlighted tile
            lightHighlightedTiles.Add(tile);
            // TODO: This will cause some tiles to appear on the list multiple times, which will make UnhighlightTiles() longer
        }

        // Highlight the medium tiles
        foreach (Tile tile in mediumTiles)
        {
            // Highlight the tile
            tile.Highligted = Tile.highlightLevels.medium;

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
            tile.Highligted = Tile.highlightLevels.heavy;

            // Mark that this is a curently highlighted tile
            heavyHighlightedTiles.Add(tile);
        }
    }

    public void UnHighlightLightTiles()
    {
        // Unhighlight each light tile
        foreach (Tile tile in lightHighlightedTiles)
        {
            tile.Highligted = Tile.highlightLevels.none;
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
                tile.Highligted = Tile.highlightLevels.light;
            }
            else
            {
                tile.Highligted = Tile.highlightLevels.none;
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
                tile.Highligted = Tile.highlightLevels.medium;
            }
            else if (lightHighlightedTiles.Contains(tile))
            {
                tile.Highligted = Tile.highlightLevels.light;
            }
            else
            {
                tile.Highligted = Tile.highlightLevels.none;
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
