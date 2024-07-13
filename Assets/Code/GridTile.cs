using UnityEngine;

public class GridTile : MonoBehaviour
{
    public GameObject highlightTile;

    [Header("Tile Variations")]
    public GameObject tileBase ;
    public GameObject tileGrass;
    public GameObject tileDirt ;
    public GameObject tileTree ;
    public GameObject tileBush ;
    public GameObject tileRock ;
    public GameObject tileWater;

    public enum TileState
    {
        Base,
        Grass,
        Dirt,
        Tree,
        Bush,
        Rock,
        Water
    }

    [Header("Tile Status")]
    public TileState state = TileState.Base;


    private Vector2 _coordinates;
    private Vector3 _highlightPos;

    private static GameObject _highlightTileObject;

    void Awake()
    {
        if (GridTile._highlightTileObject == null)
        {
            _highlightTileObject = Instantiate(highlightTile, Vector3.zero, highlightTile.transform.rotation);
            _highlightTileObject.name = "Highlight Tile";
            _highlightTileObject.SetActive(false);
        }
    }

    void Start()
    {
        _highlightPos = new Vector3(gameObject.transform.position.x, 0.3f, gameObject.transform.position.z);
    }

    public void Initialize(int q, int r)
    {
        gameObject.name = "HexTile (" + q + " ," + r + ")";
        _coordinates = new Vector2(q, r);
    }

    void OnMouseEnter()
    {
        if (Input.GetMouseButton(0))
        {
            ToggleTileState();
        }

        _highlightTileObject.SetActive(true);
        _highlightTileObject.transform.position = _highlightPos;
        _highlightTileObject.name = "Highlight Tile (" + _coordinates.x + " ," + _coordinates.y + ")";
    }

    void OnMouseExit()
    {
        if (Input.GetMouseButton(0) == false)
        {
            _highlightTileObject.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        ToggleTileState();
    }

    private void ToggleTileState()
    {
        TileState newState = HUDManager.selectedType;

        // Override UI Selectrion By Holding Down Keyboard Numbers
        if (Input.GetKey(KeyCode.Alpha1)) newState = TileState.Dirt ;
        if (Input.GetKey(KeyCode.Alpha2)) newState = TileState.Grass;
        if (Input.GetKey(KeyCode.Alpha3)) newState = TileState.Tree ;
        if (Input.GetKey(KeyCode.Alpha4)) newState = TileState.Bush ;
        if (Input.GetKey(KeyCode.Alpha5)) newState = TileState.Rock ;
        if (Input.GetKey(KeyCode.Alpha6)) newState = TileState.Water;
        if (Input.GetKey(KeyCode.Alpha0)) newState = TileState.Base ;

        SwapTile(newState);
    }

    public void SwapTile(GridTile.TileState targetState)
    {
        state = targetState;

        if (tileBase  != null)  tileBase.SetActive(TileState.Base  == state);
        if (tileGrass != null) tileGrass.SetActive(TileState.Grass == state);
        if (tileDirt  != null)  tileDirt.SetActive(TileState.Dirt  == state);
        if (tileTree  != null)  tileTree.SetActive(TileState.Tree  == state);
        if (tileBush  != null)  tileBush.SetActive(TileState.Bush  == state);
        if (tileRock  != null)  tileRock.SetActive(TileState.Rock  == state);
        if (tileWater != null) tileWater.SetActive(TileState.Water == state);
    }
}
