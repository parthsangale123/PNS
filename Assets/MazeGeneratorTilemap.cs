using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class MazeGeneratorTilemap : MonoBehaviour
{
    [Header("Maze Size")]
    public int width = 31;
    public int height = 31;

    [Header("Tilemap + Tiles")]
    public Tilemap tilemap;
    public TileBase floorTile;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public Transform wallParent;
    public GameObject doorPrefab;
    public GameObject zombiePrefab;
    public GameObject breathingStationPrefab;
    public GameObject endPointPrefab;

    [Header("Object Counts")]
    public int doorCount = 4;
    public int breathingStationCount = 4;

    [Header("Custom Puzzle Objects")]
    public List<GameObject> customPrefabs; // These should be singular unique prefabs like chappal, umbrella, etc.

    [Header("Zombie Spawning")]
    public float spawnCheckInterval = 2f;
    public static int zombiesAlive = 0;

    [Header("References")]
    public Transform player;

    private int[,] maze;
    private List<Vector2Int> floorTiles = new();
    private const float tileSize = 21f;

    void Start()
    {
        if (width % 2 == 0) width += 1;
        if (height % 2 == 0) height += 1;

        maze = new int[height, width];
        GenerateMaze();
        DrawMaze();

        if (player != null && zombiePrefab != null)
            StartCoroutine(ZombieSpawnLoop());
    }

    void GenerateMaze()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                maze[y, x] = 0;

        maze[1, 1] = 1;
        DFS(1, 1);
    }

    void DFS(int x, int y)
    {
        int[] dx = { 0, 2, 0, -2 };
        int[] dy = { -2, 0, 2, 0 };
        List<int> dirs = new List<int> { 0, 1, 2, 3 };
        Shuffle(dirs);

        foreach (int i in dirs)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1 && maze[ny, nx] == 0)
            {
                maze[y + dy[i] / 2, x + dx[i] / 2] = 1;
                maze[ny, nx] = 1;
                DFS(nx, ny);
            }
        }
    }

    void DrawMaze()
    {
        tilemap.ClearAllTiles();
        floorTiles.Clear();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3Int tilePos = new Vector3Int(x, -y, 0);
                Vector2Int gridPos = new Vector2Int(x, y);

                if (maze[y, x] == 1)
                {
                    tilemap.SetTile(tilePos, floorTile);
                    floorTiles.Add(gridPos);
                }
                else if (wallPrefab != null)
                {
                    Vector3 worldPos = tilemap.GetCellCenterWorld(tilePos);
                    Instantiate(wallPrefab, worldPos, Quaternion.identity, wallParent);
                }
            }
        }

        PlaceStartAndEnd();
        PlaceSpecialObjects();
    }

    void PlaceStartAndEnd()
    {
        Vector2Int start = new Vector2Int(1, 1);
        Vector2Int end = new Vector2Int(width - 2, height - 2);

        tilemap.SetTile(new Vector3Int(start.x, -start.y, 0), floorTile);
        tilemap.SetTile(new Vector3Int(end.x, -end.y, 0), floorTile);

        if (player != null)
            player.position = tilemap.GetCellCenterWorld(new Vector3Int(start.x, -start.y, 0));

        if (endPointPrefab != null)
            Instantiate(endPointPrefab, tilemap.GetCellCenterWorld(new Vector3Int(end.x, -end.y, 0)), Quaternion.identity);
    }

    void PlaceSpecialObjects()
    {
        List<Vector2Int> usedPositions = new();
        int minTileDistance = 3;

        bool IsFarEnough(Vector2Int pos)
        {
            foreach (var used in usedPositions)
            {
                if (Vector2Int.Distance(pos, used) < minTileDistance)
                    return false;
            }
            return true;
        }

        Vector2Int start = new Vector2Int(1, 1);
        Vector2Int end = new Vector2Int(width - 2, height - 2);

        Shuffle(floorTiles);

        List<Vector2Int> doorCandidates = new(floorTiles);
        Vector2Int doorNearStart = GetClosestFreeTile(start, doorCandidates, usedPositions, 2);
        usedPositions.Add(doorNearStart);
        InstantiateAtGrid(doorPrefab, doorNearStart);

        Vector2Int doorNearEnd = GetClosestFreeTile(end, doorCandidates, usedPositions, 2);
        usedPositions.Add(doorNearEnd);
        InstantiateAtGrid(doorPrefab, doorNearEnd);

        int doorsPlaced = 2, stationsPlaced = 0;

        foreach (var tile in floorTiles)
        {
            if (!IsFarEnough(tile) || usedPositions.Contains(tile)) continue;

            if (doorsPlaced < doorCount)
            {
                InstantiateAtGrid(doorPrefab, tile);
                usedPositions.Add(tile);
                doorsPlaced++;
                continue;
            }

            if (stationsPlaced < breathingStationCount && Vector2Int.Distance(tile, start) > minTileDistance)
            {
                InstantiateAtGrid(breathingStationPrefab, tile);
                usedPositions.Add(tile);
                stationsPlaced++;
                continue;
            }

            if (doorsPlaced >= doorCount && stationsPlaced >= breathingStationCount)
                break;
        }

        // Place custom singular resources (no green-green)
        Shuffle(floorTiles);
        HashSet<GameObject> placedPrefabs = new();

        foreach (var tile in floorTiles)
        {
            if (!IsFarEnough(tile) || usedPositions.Contains(tile)) continue;

            foreach (var prefab in customPrefabs)
            {
                if (!placedPrefabs.Contains(prefab))
                {
                    InstantiateAtGrid(prefab, tile);
                    usedPositions.Add(tile);
                    placedPrefabs.Add(prefab);
                    break;
                }
            }

            if (placedPrefabs.Count >= customPrefabs.Count)
                break;
        }
    }

    IEnumerator ZombieSpawnLoop()
    {
        while (true)
        {   
            yield return new WaitForSeconds(0.5f);

            if (player == null) continue;
            PlayerController pc = player.GetComponent<PlayerController>();
            
            if (pc == null) continue;

            int maxZombies = Mathf.CeilToInt(pc.GetMaxZombieCap());
            if (zombiesAlive >= maxZombies) continue;

            List<Vector2Int> validTiles = new();
            Vector3 playerPos = player.position;

            foreach (var tile in floorTiles)
            {
                Vector3 worldPos = GetWorldPosition(tile);
                float dist = Vector3.Distance(worldPos, playerPos);
                if (dist >= 3 * tileSize && dist <= 7* tileSize)
                    validTiles.Add(tile);
            }

            Shuffle(validTiles);

            foreach (var tile in validTiles)
            {
                if (zombiesAlive >= maxZombies) break;

                Vector3Int cell = new Vector3Int(tile.x, -tile.y, 0);
                Vector3 spawnPos = tilemap.GetCellCenterWorld(cell);
                Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
                zombiesAlive++;
            }
        }
    }

    Vector2Int GetClosestFreeTile(Vector2Int target, List<Vector2Int> pool, List<Vector2Int> exclude, int skip = 2)
    {
        List<Vector2Int> sorted = new List<Vector2Int>(pool);
        sorted.Sort((a, b) => Vector2Int.Distance(a, target).CompareTo(Vector2Int.Distance(b, target)));

        foreach (var tile in sorted)
        {
            if (exclude.Contains(tile)) continue;
            if (skip > 0)
            {
                skip--;
                continue;
            }
            return tile;
        }

        return sorted[0];
    }

    void InstantiateAtGrid(GameObject prefab, Vector2Int gridPos)
    {
        Vector3Int cellPos = new Vector3Int(gridPos.x, -gridPos.y, 0);
        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
        Instantiate(prefab, worldPos, Quaternion.identity);
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        Vector3Int cellPos = new Vector3Int(gridPos.x, -gridPos.y, 0);
        return tilemap.GetCellCenterWorld(cellPos);
    }

    public List<Vector2Int> GetFloorTilePositions()
    {
        return new List<Vector2Int>(floorTiles);
    }
}
