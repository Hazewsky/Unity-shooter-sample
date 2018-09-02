using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//nav mesh obstacle -> carve = auto detect on navmesh
[RequireComponent (typeof (Transform))]
public class MapGenerator : MonoBehaviour {
    
    //Transforms
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform mapFloor;
    public Transform navmeshMaskPrefab;
    Transform[,] tileMap;
    //Sizes
    
    public Vector2 maxMapSize;

    [Range(0,1)]
    public float outlinePercent;

    public float tileSize;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    //Map
    Map currentMap;
    public Map[] maps;
    public int mapIndex;

    void OnNewVawe(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    private void Awake()
    {
        //Spwan
        FindObjectOfType<Spawner>().OnNewWave += OnNewVawe;
    }
    public void GenerateMap()
    {
       
        //Map info
        currentMap = maps[mapIndex];
        System.Random prng = new System.Random(currentMap.seed);
        //tile map
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        
        //Coords
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));
        //HOLDERS
        //object name
        string holderName = "Generated Map";
        string tileName = "Tiles";
        string obstacleName = "Obstacles";

        if (transform.Find(holderName))
        {
            if (transform.Find(holderName).Find(tileName)){
                DestroyImmediate(transform.Find(holderName).Find(tileName).gameObject);
            }
            if (transform.Find(holderName).Find(obstacleName))
            {
                DestroyImmediate(transform.Find(holderName).Find(obstacleName).gameObject);
            }
            //immediate cause it is called from the editor
            DestroyImmediate(transform.Find(holderName).gameObject); 
        }
        //map group
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        //tiles group
       
        Transform tileHolder = new GameObject(tileName).transform;
        tileHolder.parent = mapHolder;
        //obstacles group
        
        Transform obstacleHolder = new GameObject(obstacleName).transform;
        obstacleHolder.parent = mapHolder;
        //CREATE TILES
        for (int x =0; x < currentMap.mapSize.x; x++)
        {
            for(int y=0; y < currentMap.mapSize.y; y++)
            {
                //left edge. +.5f to put edge of the tile on that pos, not center
                Vector3 tilePos = CoordToPos(x, y);
                //translate ar Euler angle. Vector.right - X Axis
                Transform newTile = Instantiate(tilePrefab, tilePos, Quaternion.Euler(Vector3.right * 90)) as Transform;
                //scale. creates distance between tiles
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = tileHolder;
                tileMap[x, y] = newTile;
            }
        }
        //CREATE OBSTACLES
        bool[,] obstacleMap = new bool[currentMap.mapSize.x,currentMap.mapSize.y];
        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObsCount = 0;
        //copy
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);
        for(int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObsCount++;
            //no obs in center. center - player spawn
            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObsCount)){
                //random height
                float obsHeight = Mathf.Lerp(currentMap.minObsHeight, currentMap.maxObsHeight, (float)prng.NextDouble());
                Vector3 obstaclePos = CoordToPos(randomCoord.x, randomCoord.y);
                Transform newObs = Instantiate(obstaclePrefab, obstaclePos + Vector3.up * obsHeight/2,
                    Quaternion.identity) as Transform;
                newObs.parent = obstacleHolder;
                newObs.localScale = new Vector3((1 - outlinePercent) * tileSize,
                    obsHeight,
                    (1 - outlinePercent) * tileSize);
                //MAKE GRADIENT COLOR
                Renderer obsRenderer = newObs.GetComponent<Renderer>();
                Material obsMat = new Material(obsRenderer.sharedMaterial);
                //see how forward randCoord is
                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obsMat.color = Color.Lerp(currentMap.fgColor, currentMap.bgColor, colorPercent);
                obsRenderer.sharedMaterial = obsMat;
                //remove obstacle coord
                allOpenCoords.Remove(randomCoord);

            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] =false;
                currentObsCount--;
            }
            
        }
        //Shuffle all open coords
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));
        //NAV MESH
        //LEFT MASK
        Transform maskLeft = Instantiate(navmeshMaskPrefab, 
            Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, 
            Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        //x scale = distance between edges of  map and maxMap
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x)/2f,1, currentMap.mapSize.y) * tileSize;

        //RIGHT MASK
        Transform maskRight = Instantiate(navmeshMaskPrefab, 
            Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, 
            Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        //x scale = distance between edges of  map and maxMap
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        //MASK TOP
        //forward - z
        Transform maskTop = Instantiate(navmeshMaskPrefab,
            Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, 
            Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        //x scale = distance between edges of  map and maxMap
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y)/2f) * tileSize;
        //MASK BOTTOM
        Transform maskBottom = Instantiate(navmeshMaskPrefab,
            Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize,
            Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        //x scale = distance between edges of  map and maxMap
        maskBottom.localScale = new Vector3(maxMapSize.x,
            1,
            (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        //x,y,0 because its rotated 90*
        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        //floor collider
        mapFloor.localScale = new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);

    }


    bool MapIsFullyAccessible(bool[,] obsMap, int currentObsCount)
    {
        bool[,] mapFlags = new bool[obsMap.GetLength(0), obsMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;
        int accessTileCount = 1;
        while(queue.Count > 0)
        {
            //1st item
            Coord tile = queue.Dequeue();
            //tile neighs
            for(int x = -1; x <=1; x++)
            {
                for(int y= -1; y <= 1; y++)
                {
                    int neighborX = tile.x + x;
                    int neighborY = tile.y + y;
                    //no diagonals
                    
                    if(x == 0 || y == 0)
                    {
                        //sure tile is on the map
                        if(neighborX >= 0 && neighborX < obsMap.GetLength(0) && 
                            neighborY >= 0 && neighborY < obsMap.GetLength(1))
                        {
                            //no check, not obstacle
                            if(!mapFlags[neighborX,neighborY] && !obsMap[neighborX, neighborY])
                            {
                                mapFlags[neighborX, neighborY] = true;
                                queue.Enqueue(new Coord(neighborX, neighborY));
                                accessTileCount++;
                            }
                        }
                    }
                }
            }
        }
        int targetAccessTileCount =(int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObsCount);
        return targetAccessTileCount == accessTileCount;
    }
    //Make method get random heigboring tile
    public Transform GetTileFromPosition(Vector3 pos)
    {
        //round down or up
        int x = Mathf.RoundToInt(pos.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(pos.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        //safety chaek
        // - 1 because arrays are 0-based, can cause crash
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    public Vector3 CoordToPos(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + .5f + x, 0, -currentMap.mapSize.y / 2f + .5f + y) * tileSize;
    }

    public Coord GetRandomCoord()
    {
        //remove 1st elem
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x,randomCoord.y];
    }

    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        public static bool operator ==(Coord c1,Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }
        public static bool operator !=(Coord c1,Coord c2)
        {
            return !(c1==c2);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObsHeight;
        public float maxObsHeight;
        public Color fgColor, bgColor;

        public Coord mapCenter
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
