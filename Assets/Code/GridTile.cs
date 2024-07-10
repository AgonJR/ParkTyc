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

    public enum TileState
    {
        Base,
        Grass,
        Dirt,
        Tree,
        Bush,
        Rock,
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
        // Temporary - Toggle Between States (Until we have a more specific way)

        TileState newState = TileState.Base;

        switch (state)
        {
            case TileState.Base:  newState = TileState.Dirt;  break;
            case TileState.Dirt:  newState = TileState.Grass; break;
            case TileState.Grass: newState = TileState.Bush;  break;
            case TileState.Bush:  newState = TileState.Rock;  break;
            case TileState.Rock:  newState = TileState.Tree;  break;
            case TileState.Tree:  newState = TileState.Base;  break;
        }

        if (Input.GetKey(KeyCode.Alpha1)) newState = TileState.Dirt ;
        if (Input.GetKey(KeyCode.Alpha2)) newState = TileState.Grass;
        if (Input.GetKey(KeyCode.Alpha3)) newState = TileState.Tree ;
        if (Input.GetKey(KeyCode.Alpha4)) newState = TileState.Bush ;
        if (Input.GetKey(KeyCode.Alpha5)) newState = TileState.Rock ;
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
    }
}
