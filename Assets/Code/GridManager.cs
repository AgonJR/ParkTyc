using System.IO;
using UnityEngine;
using System.Collections.Generic;
using FMODUnity;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("Data")]
    public GameObject tilePrefab;
    public CameraController mCam;
    public GameObject brdrPrefab;
    [Space]
    public EventReference undoSFX;

    [Header("Dev Tools")]
    public int gridSizeQ = 9;
    public int gridSizeR = 6;
    public int outerSize = 3;
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

        if (Application.isEditor)
        {
            if ( Input.GetKeyDown(KeyCode.K) ) { IncreaseGridSize(1, 0); }
            if ( Input.GetKeyDown(KeyCode.J) ) { IncreaseGridSize(0, 1); }
            if ( Input.GetKeyDown(KeyCode.L) ) { IncreaseGridSize(1, 1); }
        }
    }

    private void ClearGrid()
    {
        regenerateGrid = false;

        if ( _gridGOs != null ) { foreach (GameObject tile in _gridGOs ) { Destroy(tile); } }
        if (_bordrGOs != null ) { foreach (GameObject tile in _bordrGOs) { Destroy(tile); } }

        _undoStack    = new Stack<TileStateHistory>();
        _gridTiles    = new GridTile[gridSizeQ, gridSizeR];
        _gridGOs      = new GameObject[gridSizeQ, gridSizeR];
        _bordrGOs     = new GameObject[((gridSizeQ + outerSize*2) * (gridSizeR + outerSize*2)) - (gridSizeQ * gridSizeR)];
        _borderGoDict = new Dictionary<KeyValuePair<int, int>, GameObject>();
    }

    private void ClearBorder()
    {
        if (_bordrGOs != null ) { foreach (GameObject tile in _bordrGOs) { Destroy(tile); } }

        _bordrGOs     = new GameObject[((gridSizeQ + outerSize*2) * (gridSizeR + outerSize*2)) - (gridSizeQ * gridSizeR)];
        _borderGoDict = new Dictionary<KeyValuePair<int, int>, GameObject>();
    }

    public void GenerateGrid()
    {
        if (gridParent == null)
        {
            gridParent = new GameObject("Grid Tiles");
            gridParent.transform.parent = this.transform;
        }

        for (int q = 0; q < gridSizeQ; q++) //column
        {
            for (int r = 0; r < gridSizeR; r++) //row
            {
                Vector3 position = CalculateTilePosition(q, r);

                GameObject tileGO = Instantiate(tilePrefab, position, tilePrefab.transform.rotation);
                GridTile   tile   = tileGO.GetComponent<GridTile>();

                tile.Initialize(q, r);
                tileGO.transform.parent = gridParent.transform;

                tile.SwapTile(_loadedTiles == null ? ChooseRandomStarterTile() : _loadedTiles[q, r], addToUndo: false);

                _gridGOs[q,r] = tileGO;
                _gridTiles[q,r] = tile;
            }
        }

        _loadedTiles = null;

        mCam.CalculateXZMinMax();
        mCam.FrameGrid(_gridGOs[0, 0].transform, _gridGOs[gridSizeQ - 1, gridSizeR - 1].transform);

        GameManager.instance.ResetScore();
    }

    public void IncreaseGridSize(int incQ, int incR)
    {
        if ( incQ < 0 || incR < 0 ) { Debug.LogError("[GridManager] IncreaseGridSize(" + incQ + " , " + incR + ") --- ! --- Can't be negative!"); }

        GameObject[,] cacheGridGOs = new GameObject[gridSizeQ+incQ, gridSizeR+incR];
        GridTile[,] cacheGridTiles = new GridTile[gridSizeQ+incQ, gridSizeR+incR];

        for (int q = 0; q < gridSizeQ; q++) //column
        {
            for (int r = 0; r < gridSizeR; r++) //row
            {
                cacheGridGOs[q, r] = _gridGOs[q, r];
                cacheGridTiles[q, r] = _gridTiles[q, r];
            }
        }
        
        List<GridTile> newDirtTiles = new List<GridTile>();

        for (int q = 0; q < gridSizeQ+incQ; q++)
        {
            for (int r = 0; r < gridSizeR+incR; r++)
            {
                if (q < gridSizeQ && r < gridSizeR ) continue;

                Vector3 position = CalculateTilePosition(q, r);

                GameObject tileGO = Instantiate(tilePrefab, position, tilePrefab.transform.rotation);
                GridTile   tile   = tileGO.GetComponent<GridTile>();

                tile.Initialize(q, r);
                tileGO.transform.parent = gridParent.transform;

                      // Check for Path Connection
                     if ( q >= gridSizeQ && cacheGridTiles[q-1, r].state == GridTile.TileState.Dirt) { newDirtTiles.Add(tile); } 
                else if ( r >= gridSizeR && cacheGridTiles[q, r-1].state == GridTile.TileState.Dirt) { newDirtTiles.Add(tile); } 

                tile.SwapTile(ChooseRandomStarterTile(), addToUndo: false);

                cacheGridGOs[q,r] = tileGO;
                cacheGridTiles[q,r] = tile;
            }
        }

        _gridGOs = cacheGridGOs;
        _gridTiles = cacheGridTiles;

        // Previous Edge Tiles Now Have More Neighbours !s
        for (int q = 0; q < gridSizeQ; q++) 
        { 
            _gridTiles[q, gridSizeR-1].GatherNeighbourRefs(true); 

            if ( _gridTiles[q, gridSizeR-1].state ==  GridTile.TileState.Dirt)
            {
                _gridTiles[q, gridSizeR-1].GetComponentInChildren<PathTile>().GatherNeighbourRefs(true);
            }
        }
        for (int r = 0; r < gridSizeR; r++) 
        { 
            _gridTiles[gridSizeQ-1, r].GatherNeighbourRefs(true); 
            
            if ( _gridTiles[gridSizeQ-1, r].state ==  GridTile.TileState.Dirt)
            {
                _gridTiles[gridSizeQ-1, r].GetComponentInChildren<PathTile>().GatherNeighbourRefs(true);
            }
        }
        
        foreach ( GridTile dirtTile in newDirtTiles )
        {
            dirtTile.SwapTile(GridTile.TileState.Dirt);
        }

        gridSizeQ += incQ;
        gridSizeR += incR;

        mCam.CalculateXZMinMax();

        NPCManager.ForceScan();
        NPCManager.instance.PingNPCsGridSizeIncreased(incQ, incR);

        ClearBorder();
        GenerateOuterGrid();
    }

    public GridTile.TileState ChooseRandomStarterTile()
    {
        GridTile.TileState chosenState = GridTile.TileState.Grass;

        int roll = Random.Range(0, 100);

             if ( roll > 95 ) chosenState = GridTile.TileState.Tree;
        else if ( roll > 80 ) chosenState = GridTile.TileState.Rock;
        else if ( roll > 55 ) chosenState = GridTile.TileState.Bush;

        return chosenState;
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

        for (int q = outerSize * -1; q < gridSizeQ + outerSize; q++)
        {
            for (int r = outerSize * -1; r < gridSizeR + outerSize; r++)
            {
                if ((q >= 0 && q < gridSizeQ) && (r >= 0 && r < gridSizeR))
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
                    else if (q > gridSizeQ)
                    {
                        sizeModifier = 1.0f / (q - gridSizeQ + 1);
                    }
                    else if (r < 0)
                    {
                        sizeModifier = 1.0f / (r * -1.0f);
                    }
                    else if (r > gridSizeR)
                    {
                        sizeModifier = 1.0f / (r - gridSizeR + 1);
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

    public int GetGridSizeQ()
    {
        return gridSizeQ;
    }

    public int GetGridSizeR()
    {
        return gridSizeR;
    }

    public void ExternalRegenerate(int newQ, int newR)
    {
        gridSizeQ = newQ > 0 ? newQ : gridSizeQ;
        gridSizeR = newR > 0 ? newR : gridSizeR;

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

    public List<GridTile> GetNeighbouringGridTilesData(GridTile tile)
    {
        return GetNeighbouringGridTilesData(tile.GetColumn(), tile.GetRow());
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

    //Returns a list of up to 6 non-null GridTile References
    public List<GridTile> GetNeighbouringGridTilesData(int q, int r)
    {
        List<GridTile> neighbours = new List<GridTile>();

        if (IsValidHexCoordinate(q + 0, r - 1)) { neighbours.Add(_gridTiles[q + 0, r - 1]); } // North
        if (IsValidHexCoordinate(q + 0, r + 1)) { neighbours.Add(_gridTiles[q + 0, r + 1]); } // South
        if (IsValidHexCoordinate(q + 1, r + (q % 2 == 0 ? 0 : 1))) { neighbours.Add(_gridTiles[q + 1, r + (q % 2 == 0 ? 0 : 1)]); } // South East
        if (IsValidHexCoordinate(q - 1, r + (q % 2 == 0 ? 0 : 1))) { neighbours.Add(_gridTiles[q - 1, r + (q % 2 == 0 ? 0 : 1)]); } // South West
        if (IsValidHexCoordinate(q + 1, r - (q % 2 == 0 ? 1 : 0))) { neighbours.Add(_gridTiles[q + 1, r - (q % 2 == 0 ? 1 : 0)]); } // North East
        if (IsValidHexCoordinate(q - 1, r - (q % 2 == 0 ? 1 : 0))) { neighbours.Add(_gridTiles[q - 1, r - (q % 2 == 0 ? 1 : 0)]); } // North West


        return neighbours;
    }

    // Finds and returns an edge tile of a specific type
    public List<GameObject> ScanEdgeTiles(GridTile.TileState tileType, Direction direction = Direction.Any)
    {
        List<GameObject> edgeTiles = new();

        if (_gridGOs != null)
        {
            // int s = (int) Mathf.Sqrt(_gridGOs.Length);

            int sQ = gridSizeQ;
            int sR = gridSizeR;

            for (int q = 0; q < sQ; q++) // column
            {
                for (int r = 0; r < sR; r++) // row
                {
                    if (direction == Direction.Any)
                    {
                        if (q == 0 || q == sQ - 1 || r == 0 || r == sR - 1)
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
                    else if (direction == Direction.East && q == sQ - 1)
                    {
                        if (_gridTiles[q, r].state == tileType) { edgeTiles.Add(_gridGOs[q, r]); }
                    }
                    else if (direction == Direction.North && r == 0 && q > 0)
                    {
                        if (_gridTiles[q, r].state == tileType) { edgeTiles.Add(_gridGOs[q, r]); }
                    }
                    else if (direction == Direction.South && r == sR - 1 && q > 0)
                    {
                        if (_gridTiles[q, r].state == tileType) { edgeTiles.Add(_gridGOs[q, r]); }
                    }
                }
            }
        }
    

        return edgeTiles;
    }

    // Returns true if 'start' and 'end' have a direct connection of a specific type (eg. Dirt Path)
    public bool CheckTileConnection(GridTile start, GridTile end, GridTile.TileState connectionType, int[,] visited = null)
    {
        if ( visited == null )
        {
            visited = new int[gridSizeQ, gridSizeR]; 
            for (int q = 0; q < gridSizeQ; q++) { for (int r = 0; r < gridSizeR; r++) { visited[q, r] = 0; } }
        }

        if ( visited[start.GetColumn(), start.GetRow()] > 0 ) return false;

        visited[start.GetColumn(), start.GetRow()] = 1;
        List<GridTile> neighbours = GetNeighbouringGridTilesData(start);
        foreach ( GridTile nTile in neighbours )
        {
            if (nTile.GetCoordinates() == end.GetCoordinates()) { return true; }
            if ( nTile.state == connectionType ) { if ( CheckTileConnection(nTile, end, connectionType, visited) ) { return true; } }
        }

        return false;
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
        else if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                DoUndo();
            }
        }
    }

    public void RemoveFromUndo(TileStateHistory tileSwapData)
    {
        // Currently used to remove tree/rock swaps when a Camp is completed

        TileStateHistory[] undoHistory = _undoStack.ToArray();

        for ( int i = 0; i < undoHistory.Length; i++ )
        {
            if (undoHistory[i].coordinates == tileSwapData.coordinates && undoHistory[i].endState == tileSwapData.endState )
            {
                _undoStack = new Stack<TileStateHistory>();

                for ( int j = 0; j < undoHistory.Length; j++ )
                {
                    if ( i != j )
                    {
                        _undoStack.Push(undoHistory[j]);
                    }
                }

                return;
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

        RuntimeManager.PlayOneShot(undoSFX, transform.position);

        _gridTiles[q, r].SwapTile(latestTileSwapData.startState, addToUndo:false);
    }

    // - - -
    // Save & Load
    // - - -

    public void SaveGridAsJSON(string fileName)
    {
        string directoryPath = Path.Combine(Application.dataPath + "/SavedGrids/");
        Directory.CreateDirectory(directoryPath);

        string filePath = Path.Combine(directoryPath, fileName + ".json");

        string txtToSave = gridSizeQ + "x" + gridSizeR + ",\n";

        for (int r = 0; r < gridSizeR; r++)
        {
            for (int q = 0; q < gridSizeQ; q++)
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
        string directoryPath = Path.Combine(Application.dataPath + "/SavedGrids/");

        string filePath = Path.Combine(directoryPath, fileName + ".json");

        if (File.Exists(filePath))
        {
            string jsonTxt = File.ReadAllText(filePath);
            jsonTxt = jsonTxt.Replace("\n", string.Empty);

            string[] jsonData = jsonTxt.Split(',');

            int loadedGridSizeQ = int.Parse(jsonData[0].Split('x')[0]);
            int loadedGridSizeR = int.Parse(jsonData[0].Split('x')[1]);

            _loadedTiles = new GridTile.TileState[loadedGridSizeQ, loadedGridSizeR];

            int i = 1;
            for (int r = 0; r < loadedGridSizeR; r++)
            {
                for (int q = 0; q < loadedGridSizeQ; q++)
                {
                    _loadedTiles[q, r] = (GridTile.TileState) int.Parse(jsonData[i++]);
                }
            }

            gridSizeQ = loadedGridSizeQ;
            gridSizeR = loadedGridSizeR;

            regenerateGrid = true;

            GameManager.instance.hudManagerRef.DebugPanel_SetUITextGridSize(loadedGridSizeQ, loadedGridSizeR);
        }
        else
        {
            Debug.LogError("[GridManager] LoadGRidFromJSON() - File not found: " + filePath);
        }

    }

    // - - -

}
