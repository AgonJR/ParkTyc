using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("Data")]
    public GameObject tilePrefab;
    public CameraController mCam;
    public GameObject brdrPrefab;

    [Header("Dev Tools")]
    public int gridSize = 3; //Assuming Square Grid
    public int outerSize= 3;
    public bool regenerateGrid = true;
    public bool shrnkOutrTiles = true;

    private GameObject[,] _gridGOs;
    private GridTile[,] _gridTiles;
    private GameObject  gridParent;

    private GameObject[] _bordrGOs;
    private GameObject bordrParent;

    private static Stack<TileStateHistory> _undoStack;

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
            GenerateOuterGrid();
        }

        CheckInputForUndo();
    }

    private void ClearGrid()
    {
        regenerateGrid = false;

        if ( _gridGOs != null ) { foreach (GameObject tile in _gridGOs ) { Destroy(tile); } }
        if (_bordrGOs != null ) { foreach (GameObject tile in _bordrGOs) { Destroy(tile); } }

        _undoStack = new Stack<TileStateHistory>();
        _gridTiles = new GridTile[gridSize, gridSize];
        _gridGOs   = new GameObject[gridSize, gridSize];
        _bordrGOs  = new GameObject[((gridSize + outerSize*2) * (gridSize + outerSize*2)) - (gridSize * gridSize)];
    }

    public void GenerateGrid()
    {
        if (gridParent == null)
        {
            gridParent = new GameObject("Grid Tiles");
            gridParent.transform.parent = this.transform;
        }

        for (int q = 0; q < gridSize; q++) //column
        {
            for (int r = 0; r < gridSize; r++) //row
            {
                Vector3 position = CalculateTilePosition(q, r);

                GameObject tileGO = Instantiate(tilePrefab, position, tilePrefab.transform.rotation);
                GridTile   tile   = tileGO.GetComponent<GridTile>();

                tile.Initialize(q, r);
                tileGO.transform.parent = gridParent.transform;
                tile.SwapTile(GridTile.TileState.Grass, addToUndo: false);

                _gridGOs[q,r] = tileGO;
                _gridTiles[q,r] = tile;
            }
        }

        mCam.CalculateXZMinMax();
        mCam.FrameGrid(_gridGOs[0, 0].transform, _gridGOs[gridSize - 1, gridSize - 1].transform);
    }


    // Out of bounds, non-player grid tiles
    public void GenerateOuterGrid()
    {
        if (bordrParent == null)
        {
            bordrParent = new GameObject("Border Tiles");
            bordrParent.transform.parent = this.transform;
        }

        int i = 0;

        for (int q = outerSize * -1; q < gridSize + outerSize; q++)
        {
            for (int r = outerSize * -1; r < gridSize + outerSize; r++)
            {
                if ((q >= 0 && q < gridSize) && (r >= 0 && r < gridSize))
                {
                    continue;
                }

                Vector3 position = CalculateTilePosition(q, r);

                GameObject tileGO = Instantiate(brdrPrefab, position, tilePrefab.transform.rotation);

                tileGO.transform.parent = bordrParent.transform;
                tileGO.name = "Outer Tile (" + q + " ," + r + ")";


                //Make tiles smaller as they move further from the base grid
                if (shrnkOutrTiles)
                {
                    float sizeModifier = 1.0f;
                    if (q < -1)
                    {
                        sizeModifier = 1.0f / (q * -1);
                    }
                    else if (q > gridSize)
                    {
                        sizeModifier = 1.0f / (q - gridSize + 1);
                    }
                    else if (r < 0)
                    {
                        sizeModifier = 1.0f / (r * -1.0f);
                    }
                    else if (r > gridSize)
                    {
                        sizeModifier = 1.0f / (r - gridSize + 1);
                    }

                    tileGO.transform.localScale = new Vector3(tileGO.transform.localScale.x * sizeModifier, tileGO.transform.localScale.y * sizeModifier, tileGO.transform.localScale.z);
                }

                _bordrGOs[i++] = tileGO;
            }
        }
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

    public static float CalculateDistance(int q1, int r1, int q2, int r2)
    {
        float distance;

        GameObject tile1 = instance._gridGOs[q1, r1];
        GameObject tile2 = instance._gridGOs[q2, r2];

        //TO DO - Implement pathfinding to account for obstacles!
        distance = Vector3.Distance(tile1.transform.position, tile2.transform.position);

        return distance;
    }

    public Vector3 CalculateTilePosition(int q, int r)
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

    public List<GameObject> GetNeighbouringTiles(GridTile tile)
    {
        return GetNeighbouringTiles(tile.GetColumn(), tile.GetRow());
    }

    public List<GameObject> GetNeighbouringTiles(int q, int r)
    {
        List<GameObject> neighbours = new List<GameObject>();

        if (IsValidHexCoordinate(q + 0, r + 1)) { neighbours.Add(_gridGOs[q + 0, r + 1]); } // North
        if (IsValidHexCoordinate(q + 0, r - 1)) { neighbours.Add(_gridGOs[q + 0, r - 1]); } // South
        if (IsValidHexCoordinate(q + 1, r + (q % 2 == 0 ? 0 : 1))) { neighbours.Add(_gridGOs[q + 1, r + (q % 2 == 0 ? 0 : 1)]); } // North East
        if (IsValidHexCoordinate(q - 1, r + (q % 2 == 0 ? 0 : 1))) { neighbours.Add(_gridGOs[q - 1, r + (q % 2 == 0 ? 0 : 1)]); } // North West
        if (IsValidHexCoordinate(q + 1, r - (q % 2 == 0 ? 1 : 0))) { neighbours.Add(_gridGOs[q + 1, r - (q % 2 == 0 ? 1 : 0)]); } // South East
        if (IsValidHexCoordinate(q - 1, r - (q % 2 == 0 ? 1 : 0))) { neighbours.Add(_gridGOs[q - 1, r - (q % 2 == 0 ? 1 : 0)]); } // South West


        return neighbours;
    }

    private bool IsValidHexCoordinate(int q, int r)
    {
        return q >= 0 && q < _gridGOs.GetLength(0) && r >= 0 && r < _gridGOs.GetLength(1);
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
                    else if (direction == Direction.North && r == 0 && q > 0)
                    {
                        if (_gridTiles[q, r].state == tileType) { edgeTiles.Add(_gridGOs[q, r]); }
                    }
                    else if (direction == Direction.South && r == s - 1 && q > 0)
                    {
                        if (_gridTiles[q, r].state == tileType) { edgeTiles.Add(_gridGOs[q, r]); }
                    }
                }
            }
        }
    

        return edgeTiles;
    }


    // - - -
    // UNDO
    // - - -

    public static void AddToUndoHistory(TileStateHistory latestTileSwap)
    {
        GridManager._undoStack.Push(latestTileSwap);
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
        if (GridManager._undoStack.Count == 0)
        {
            return;
        }

        TileStateHistory latestTileSwapData = GridManager._undoStack.Pop();

        int q = (int) latestTileSwapData.coordinates.x;
        int r = (int) latestTileSwapData.coordinates.y;

        _gridTiles[q, r].SwapTile(latestTileSwapData.startState, addToUndo:false);
    }

    // - - -
}
