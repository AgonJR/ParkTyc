using System.IO;
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

    private GameObject[]  _bordrGOs;
    private GameObject _bordrParent;
    private GridTile.TileState[,] _loadedTiles;
    private Dictionary<KeyValuePair<int, int>, GameObject> _borderGoDict;

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

        _undoStack    = new Stack<TileStateHistory>();
        _gridTiles    = new GridTile[gridSize, gridSize];
        _gridGOs      = new GameObject[gridSize, gridSize];
        _bordrGOs     = new GameObject[((gridSize + outerSize*2) * (gridSize + outerSize*2)) - (gridSize * gridSize)];
        _borderGoDict = new Dictionary<KeyValuePair<int, int>, GameObject>();
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
                tile.SwapTile(_loadedTiles == null ? GridTile.TileState.Grass : _loadedTiles[q, r], addToUndo: false);

                _gridGOs[q,r] = tileGO;
                _gridTiles[q,r] = tile;
            }
        }

        _loadedTiles = null;

        mCam.CalculateXZMinMax();
        mCam.FrameGrid(_gridGOs[0, 0].transform, _gridGOs[gridSize - 1, gridSize - 1].transform);
    }


    // Out of bounds, non-player grid tiles
    public void GenerateOuterGrid()
    {
        if (_bordrParent == null)
        {
            _bordrParent = new GameObject("Border Tiles");
            _bordrParent.transform.parent = this.transform;
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

                tileGO.transform.parent = _bordrParent.transform;
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

                _borderGoDict.Add(new KeyValuePair<int, int>(q, r), tileGO);
            }
        }
    }

    public GameObject GetBorderTileGO(int q, int r)
    {
        return _borderGoDict[new KeyValuePair<int, int>(q, r)];
    }

    public int GetGridSize()
    {
        return gridSize;
    }

    public void ExternalRegenerate(int newSize = -1)
    {
        gridSize = newSize > 0 ? newSize : gridSize;
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

    public List<GameObject> GetNeighbouringTilesConst(GridTile tile)
    {
        return GetNeighbouringTilesConst(tile.GetColumn(), tile.GetRow());
    }

    private bool IsValidHexCoordinate(int q, int r)
    {
        return q >= 0 && q < _gridGOs.GetLength(0) && r >= 0 && r < _gridGOs.GetLength(1);
    }

    //Returns a list of up to 6 non-null GameObjects
    public List<GameObject> GetNeighbouringTiles(int q, int r)
    {
        List<GameObject> neighbours = new List<GameObject>();

        if (IsValidHexCoordinate(q + 0, r - 1)) { neighbours.Add(_gridGOs[q + 0, r - 1]); } // North
        if (IsValidHexCoordinate(q + 0, r + 1)) { neighbours.Add(_gridGOs[q + 0, r + 1]); } // South
        if (IsValidHexCoordinate(q + 1, r + (q % 2 == 0 ? 0 : 1))) { neighbours.Add(_gridGOs[q + 1, r + (q % 2 == 0 ? 0 : 1)]); } // South East
        if (IsValidHexCoordinate(q - 1, r + (q % 2 == 0 ? 0 : 1))) { neighbours.Add(_gridGOs[q - 1, r + (q % 2 == 0 ? 0 : 1)]); } // South West
        if (IsValidHexCoordinate(q + 1, r - (q % 2 == 0 ? 1 : 0))) { neighbours.Add(_gridGOs[q + 1, r - (q % 2 == 0 ? 1 : 0)]); } // North East
        if (IsValidHexCoordinate(q - 1, r - (q % 2 == 0 ? 1 : 0))) { neighbours.Add(_gridGOs[q - 1, r - (q % 2 == 0 ? 1 : 0)]); } // North West


        return neighbours;
    }

    //Returns a list of exactly 6 GameObjects, including null
    public List<GameObject> GetNeighbouringTilesConst(int q, int r)
    {
        List<GameObject> neighbours = new List<GameObject>();

        for (int i = 0; i < 6; i++) { neighbours.Add(null); }

        if (IsValidHexCoordinate(q + 0, r - 1)) { neighbours[0] = _gridGOs[q + 0, r - 1]; } // North
        if (IsValidHexCoordinate(q + 0, r + 1)) { neighbours[1] = _gridGOs[q + 0, r + 1]; } // South
        if (IsValidHexCoordinate(q + 1, r + (q % 2 == 0 ? 0 : 1))) { neighbours[2] = _gridGOs[q + 1, r + (q % 2 == 0 ? 0 : 1)]; } // South East
        if (IsValidHexCoordinate(q - 1, r + (q % 2 == 0 ? 0 : 1))) { neighbours[3] = _gridGOs[q - 1, r + (q % 2 == 0 ? 0 : 1)]; } // South West
        if (IsValidHexCoordinate(q + 1, r - (q % 2 == 0 ? 1 : 0))) { neighbours[4] = _gridGOs[q + 1, r - (q % 2 == 0 ? 1 : 0)]; } // North East
        if (IsValidHexCoordinate(q - 1, r - (q % 2 == 0 ? 1 : 0))) { neighbours[5] = _gridGOs[q - 1, r - (q % 2 == 0 ? 1 : 0)]; } // North West


        return neighbours;
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
    // Save & Load
    // - - -

    public void SaveGridAsJSON(string fileName)
    {
        string directoryPath = Path.Combine(Application.dataPath + "/SavedGrids");
        Directory.CreateDirectory(directoryPath);

        string filePath = Path.Combine(directoryPath, fileName + ".json");

        string txtToSave = gridSize + ",\n";

        for (int r = 0; r < gridSize; r++)
        {
            for (int q = 0; q < gridSize; q++)
            {
                txtToSave += (int)_gridTiles[q, r].state + ",";
            }
            txtToSave += "\n";
        }

        txtToSave = txtToSave.TrimEnd('\n');
        txtToSave = txtToSave.TrimEnd(',');

        File.WriteAllText(filePath, txtToSave);
    }

    public void LoadGridFromJSON(string fileName)
    {
        string directoryPath = Path.Combine(Application.dataPath + "/SavedGrids");

        string filePath = Path.Combine(directoryPath, fileName + ".json");

        if (File.Exists(filePath))
        {
            string jsonTxt = File.ReadAllText(filePath);
            jsonTxt = jsonTxt.Replace("\n", string.Empty);

            string[] jsonData = jsonTxt.Split(',');

            int loadedGridSize = int.Parse(jsonData[0]);

            _loadedTiles = new GridTile.TileState[loadedGridSize, loadedGridSize];

            int i = 1;
            for (int r = 0; r < loadedGridSize; r++)
            {
                for (int q = 0; q < loadedGridSize; q++)
                {
                    _loadedTiles[q, r] = (GridTile.TileState) int.Parse(jsonData[i++]);
                }
            }

            gridSize = loadedGridSize;

            regenerateGrid = true;

            GameManager.instance.hudManagerRef.DebugPanel_SetUITextGridSize(loadedGridSize);
        }
        else
        {
            Debug.LogError("[GridManager] LoadGRidFromJSON() - File not found: " + filePath);
        }

    }

    // - - -

}
