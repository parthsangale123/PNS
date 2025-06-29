using UnityEngine;
using System.Collections.Generic;

public class MazeManager : MonoBehaviour
{
    public static MazeManager Instance;

    public MazeGeneratorTilemap mazeGenerator;

    [HideInInspector] public List<Vector2Int> walkableTiles = new();

    private void Awake()
    {
        Instance = this;
    }

    public void RandomizePositions(out Vector3 zombiePos, out Vector3 playerPos)
    {
        if (mazeGenerator == null) Debug.LogError("MazeGenerator not assigned!");
        walkableTiles = mazeGenerator.GetFloorTilePositions();

        Vector2Int zombieGrid = GetRandomTile();
        Vector2Int playerGrid = GetRandomTileFarFrom(zombieGrid);

        zombiePos = mazeGenerator.GetWorldPosition(zombieGrid);
        playerPos = mazeGenerator.GetWorldPosition(playerGrid);
    }

    private Vector2Int GetRandomTile()
    {
        return walkableTiles[Random.Range(0, walkableTiles.Count)];
    }

    private Vector2Int GetRandomTileFarFrom(Vector2Int reference)
    {
        float minDistance = 8f;
        foreach (var tile in walkableTiles)
        {
            if (Vector2Int.Distance(tile, reference) > minDistance)
                return tile;
        }

        return GetRandomTile(); // fallback
    }
}
