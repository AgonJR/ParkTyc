using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("Data")]
    public GameObject tilePrefab;
    public CameraController mCam;

    [Header("Dev Tools")]
    public int gridSize = 3; //Assuming Square Grid
    public bool regenerateGrid = true;

    private GameObject[,] _gridGOs;
    private GridTile[,] _gridTiles;

    private static Stack<TileStateHistory> undoStack;

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

        CheckInputForUndo();
    }

    private void ClearGrid()
    {
        regenerateGrid = false;

        if ( _gridGOs != null ) { foreach (GameObject tile in _gridGOs) { Destroy(tile); } }

        _gridGOs   = new GameObject[gridSize, gridSize];
        _gridTiles = new GridTile[gridSize, gridSize];
        undoStack  = new Stack<TileStateHistory>();
    }

    public void GenerateGrid()
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
                tile.SwapTile(GridTile.TileState.Grass, addToUndo: false);

                _gridGOs[q,r] = tileGO;
                _gridTiles[q,r] = tile;
            }
        }

        mCam.CalculateXZMinMax();
        mCam.FrameGrid(_gridGOs[0, 0].transform, _gridGOs[gridSize - 1, gridSize - 1].transform);
    }

    // For String Input From Debug Panel
    public void SetGridSize(string newSize)
    {
        gridSize = int.Parse(newSize);
    }

    public int GetGridSize()
    {
        return gridSize;
    }

    public void ExternalRegenerate()
    {
        regenerateGrid = true;
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
    public List<GameObject> ScanEdgeTiles(GridTile.TileState tileType, Direction direction = Direction.Any)
    {
        List<GameObject> edgeTiles = new();

        if (_gridGOs != null)
        {
            int s = (int) Mathf.Sqrt(_gridGOs.Length);

            for (int q = 0; q < s; q++) //column
            {
                for (int r = 0; r < s; r++) //row
                {
                    if (direction == Direction.Any)
                    {
                        if (q == 0 || q == s - 1 || r == 0 || r == s - 1)
                        {
                            if (_gridTiles[q, r].state == tileType)
                            {
                                edgeTiles.Add(_gridGOs[q, r]);
                            }
                        }
                    }
                    else if (direction == Direction.West && q == 0)
                    {
                        if (_gridTiles[q, r].state == tileType) { edgeTiles.Add(_gridGOs[q, r]); }
                    }
                    else if (direction == Direction.East && q == s - 1)
                    {
                        if (_gridTiles[q, r].state == tileType) { edgeTiles.Add(_gridGOs[q, r]); }
                    }
                    else if (direction == Direction.North && r == 0)
                    {
                        if (_gridTiles[q, r].state == tileType) { edgeTiles.Add(_gridGOs[q, r]); }
                    }
                    else if (direction == Direction.South && r == s - 1)
                    {
                        if (_gridTiles[q, r].state == tileType) { edgeTiles.Add(_gridGOs[q, r]); }
                    }
                }
            }
        }
    

        return edgeTiles;
    }

    public static void AddToUndoHistory(TileStateHistory latestTileSwap)
    {
        GridManager.undoStack.Push(latestTileSwap);
    }

    public void CheckInputForUndo()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand))
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                DoUndo();
            }
        }
    }

    public void DoUndo()
    {
        if (GridManager.undoStack.Count == 0)
        {
            return;
        }

        TileStateHistory latestTileSwapData = GridManager.undoStack.Pop();

        int q = (int) latestTileSwapData.coordinates.x;
        int r = (int) latestTileSwapData.coordinates.y;

        _gridTiles[q, r].SwapTile(latestTileSwapData.startState, addToUndo:false);
    }
}
