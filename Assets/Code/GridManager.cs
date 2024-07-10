using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("Data")]
    public GameObject tilePrefab;

    [Header("Dev Tools")]
    public int gridSize = 3; //Assuming Square Grid
    public bool regenerateGrid = true;

    private GameObject[,] _gridGOs;
    private GridTile[,] _gridTiles;

    public enum Direction
    {
        North,
        South,
        East,
        West,
        Any
    }

    private void Awake()
    {
        GridManager.instance = this;
    }

    private void Update()
    {
        if (regenerateGrid)
        {
            ClearGrid();
            GenerateGrid();
        }
    }

    private void ClearGrid()
    {
        regenerateGrid = false;

        if ( _gridGOs != null ) { foreach (GameObject tile in _gridGOs) { Destroy(tile); } }

        _gridGOs = new GameObject[gridSize, gridSize];
        _gridTiles = new GridTile[gridSize, gridSize];
    }

    private void GenerateGrid()
    {

        for (int q = 0; q < gridSize; q++) //column
        {
            for (int r = 0; r < gridSize; r++) //row
            {
                Vector3 position = CalculateTilePosition(q, r);

                GameObject tileGO = Instantiate(tilePrefab, position, tilePrefab.transform.rotation);
                GridTile   tile   = tileGO.GetComponent<GridTile>();

                tile.Initialize(q, r);
                tileGO.transform.parent = this.transform;

                _gridGOs[q,r] = tileGO;
                _gridTiles[q,r] = tile;
            }
        }
    }

    private Vector3 CalculateTilePosition(int q, int r)
    {
        //Hardcoded values match placeholder grid tile

        float h = -8.70f;
        float w = 7.535f;

        float oddOffset = -4.35f;

        float x = q * w; 
        float z = r * h;

        z += q % 2 == 0.0f ? 0.0f : oddOffset;

        return new Vector3(x, 0.0f, z);
    }

    // Finds and returns an edge tile of a specific type
    public GameObject ScanEdgeTiles(GridTile.TileState tileType, Direction direction = Direction.Any)
    {
        if (_gridGOs != null)
        {
            for (int q = 0; q < gridSize; q++) //column
            {
                for (int r = 0; r < gridSize; r++) //row
                {
                    if (direction == Direction.Any)
                    {
                        if (q == 0 || q == gridSize - 1 || r == 0 || r == gridSize - 1)
                        {
                            if (_gridTiles[q, r].state == tileType)
                            {
                                return _gridGOs[q, r];
                            }
                        }
                    }
                    else if (direction == Direction.West && q == 0)
                    {
                        if (_gridTiles[q, r].state == tileType) { return _gridGOs[q, r]; }
                    }
                    else if (direction == Direction.East && q == gridSize - 1)
                    {
                        if (_gridTiles[q, r].state == tileType) { return _gridGOs[q, r]; }
                    }
                    else if (direction == Direction.North && r == 0)
                    {
                        if (_gridTiles[q, r].state == tileType) { return _gridGOs[q, r]; }
                    }
                    else if (direction == Direction.South && r == gridSize - 1)
                    {
                        if (_gridTiles[q, r].state == tileType) { return _gridGOs[q, r]; }
                    }
                }
            }
        }
    

        return null;
    }
}
