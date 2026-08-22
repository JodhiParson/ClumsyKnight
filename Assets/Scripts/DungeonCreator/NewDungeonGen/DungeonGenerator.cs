using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedural dungeon generator: places non-overlapping rectangular rooms,
/// connects them with L-shaped corridors, then surrounds every floor tile
/// with walls. Works with any prefabs (cubes, tiles, meshes, etc).
///
/// Setup:
/// 1. Create an empty GameObject, attach this script.
/// 2. Create a "Floor" prefab (e.g. a flat Cube/Plane, scale ~1x0.1x1) and a
///    "Wall" prefab (e.g. a Cube scaled ~1x2x1) and assign them in the Inspector.
/// 3. Create an empty child GameObject named "DungeonParent", assign it to
///    Dungeon Parent (keeps generated objects organized and easy to clear).
/// 4. Press Play, or right-click the component header and choose
///    "Generate Dungeon" to regenerate in the Editor.
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 60;
    public int height = 40;
    public float cellSize = 1f;

    [Header("Room Settings")]
    public int maxRooms = 15;
    public Vector2Int roomSizeMin = new Vector2Int(4, 4);
    public Vector2Int roomSizeMax = new Vector2Int(10, 10);
    [Tooltip("How many times the algorithm tries to place a room before giving up.")]
    public int placementAttempts = 200;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public Transform dungeonParent;

    [Header("Random Seed")]
    public bool useRandomSeed = true;
    public int seed = 0;

    private enum TileType { None, Floor, Wall }
    private TileType[,] grid;
    private readonly List<RectInt> rooms = new List<RectInt>();

    void Start()
    {
        GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        ClearDungeon();

        if (useRandomSeed) seed = System.Environment.TickCount;
        Random.InitState(seed);

        grid = new TileType[width, height];
        rooms.Clear();

        PlaceRooms();
        ConnectRooms();
        PlaceWalls();
        InstantiateDungeon();
    }

    /// <summary>Public wrapper so editor scripts / UI buttons can clear without regenerating.</summary>
    public void ClearDungeonPublic()
    {
        ClearDungeon();
    }

    void ClearDungeon()
    {
        if (dungeonParent == null) return;
        for (int i = dungeonParent.childCount - 1; i >= 0; i--)
        {
            Transform child = dungeonParent.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(child.gameObject);
                continue;
            }
#endif
            Destroy(child.gameObject);
        }
    }

    // --- Room placement -----------------------------------------------

    void PlaceRooms()
    {
        for (int i = 0; i < placementAttempts && rooms.Count < maxRooms; i++)
        {
            int w = Random.Range(roomSizeMin.x, roomSizeMax.x + 1);
            int h = Random.Range(roomSizeMin.y, roomSizeMax.y + 1);
            int x = Random.Range(1, width - w - 1);
            int y = Random.Range(1, height - h - 1);

            RectInt newRoom = new RectInt(x, y, w, h);

            bool overlaps = false;
            foreach (var r in rooms)
            {
                // Pad existing rooms by 1 so rooms never touch (leaves room for walls).
                RectInt padded = new RectInt(r.x - 1, r.y - 1, r.width + 2, r.height + 2);
                if (padded.Overlaps(newRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }
    }

    void CarveRoom(RectInt room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
            for (int y = room.y; y < room.y + room.height; y++)
                grid[x, y] = TileType.Floor;
    }

    // --- Corridors -------------------------------------------------------

    void ConnectRooms()
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int a = Vector2Int.RoundToInt(rooms[i - 1].center);
            Vector2Int b = Vector2Int.RoundToInt(rooms[i].center);

            // Randomize corridor bend direction so it's not always the same L-shape.
            if (Random.value < 0.5f)
            {
                CarveHorizontalCorridor(a.x, b.x, a.y);
                CarveVerticalCorridor(a.y, b.y, b.x);
            }
            else
            {
                CarveVerticalCorridor(a.y, b.y, a.x);
                CarveHorizontalCorridor(a.x, b.x, b.y);
            }
        }
    }

    void CarveHorizontalCorridor(int x1, int x2, int y)
    {
        int start = Mathf.Min(x1, x2);
        int end = Mathf.Max(x1, x2);
        for (int x = start; x <= end; x++)
            SetFloorSafe(x, y);
    }

    void CarveVerticalCorridor(int y1, int y2, int x)
    {
        int start = Mathf.Min(y1, y2);
        int end = Mathf.Max(y1, y2);
        for (int y = start; y <= end; y++)
            SetFloorSafe(x, y);
    }

    void SetFloorSafe(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        grid[x, y] = TileType.Floor;
    }

    // --- Walls -------------------------------------------------------

    void PlaceWalls()
    {
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != TileType.Floor) continue;

                // Any empty (None) tile touching a floor tile (including diagonals) becomes a wall.
                for (int dir = 0; dir < 8; dir++)
                {
                    int nx = x + dx[dir];
                    int ny = y + dy[dir];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    if (grid[nx, ny] == TileType.None)
                        grid[nx, ny] = TileType.Wall;
                }
            }
        }
    }

    // --- Instantiation -------------------------------------------------------

    void InstantiateDungeon()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);

                if (grid[x, y] == TileType.Floor && floorPrefab != null)
                {
                    Instantiate(floorPrefab, pos, Quaternion.identity, dungeonParent);
                }
                else if (grid[x, y] == TileType.Wall && wallPrefab != null)
                {
                    // Raise walls half their height so they sit on top of the floor plane.
                    Instantiate(wallPrefab, pos, Quaternion.identity, dungeonParent);
                }
            }
        }
    }

    // --- Debug visualization (works even without prefabs assigned) -------

    void OnDrawGizmosSelected()
    {
        if (grid == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);
                if (grid[x, y] == TileType.Floor)
                    Gizmos.color = Color.gray;
                else if (grid[x, y] == TileType.Wall)
                    Gizmos.color = Color.black;
                else
                    continue;

                Gizmos.DrawCube(pos, Vector3.one * cellSize * 0.9f);
            }
        }
    }
}