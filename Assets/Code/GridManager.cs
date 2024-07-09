using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [Header("Data")]
    public GameObject tilePrefab;

    [Header("Dev Tools")]
    public int gridSize = 3; //Assuming Square Grid
    public bool regenerateGrid = true;

    private GameObject[,] _grid;

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

        if ( _grid != null ) { foreach (GameObject tile in _grid) { Destroy(tile); } }

        _grid = new GameObject[gridSize, gridSize];
    }

    private void GenerateGrid()
    {

        for (int q = 0; q < gridSize; q++) //column
        {
            for (int r = 0; r < gridSize; r++) //row
            {
                Vector3 position = CalculateTilePosition(q, r);

                GameObject tile = Instantiate(tilePrefab, position, tilePrefab.transform.rotation);

                tile.transform.parent = this.transform;
                tile.GetComponent<GridTile>().Initialize(q, r);

                _grid[q,r] = tile;
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
}
