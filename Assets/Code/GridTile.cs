using UnityEngine;
using System.Collections.Generic;

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

    public static readonly Dictionary<TileState, int> stateWalkScores = new()
    {
        { TileState.Base,  9 },
        { TileState.Grass, 5 },
        { TileState.Dirt,  1 },
        { TileState.Tree, -1 },
        { TileState.Bush, -1 },
        { TileState.Rock, -1 },
        { TileState.Water,-1 }
    };

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
        SwapTile(HUDManager.selectedType);
    }

    public void SwapTile(GridTile.TileState targetState, bool addToUndo = true)
    {
        if ( addToUndo ) GridManager.AddToUndoHistory(new TileStateHistory(_coordinates, state, targetState));

        state = targetState;

        if (tileBase  != null)  tileBase.SetActive(TileState.Base  == state);
        if (tileGrass != null) tileGrass.SetActive(TileState.Grass == state);
        if (tileDirt  != null)  tileDirt.SetActive(TileState.Dirt  == state);
        if (tileTree  != null)  tileTree.SetActive(TileState.Tree  == state);
        if (tileBush  != null)  tileBush.SetActive(TileState.Bush  == state);
        if (tileRock  != null)  tileRock.SetActive(TileState.Rock  == state);
        if (tileWater != null) tileWater.SetActive(TileState.Water == state);

        //// Debug - Testing GetNeighboringTiles()
        //if (targetState == TileState.Bush)
        //{
        //    List<GameObject> neighbourGOs = GridManager.instance.GetNeighbouringTiles((int)_coordinates.x, (int)_coordinates.y);
        //    for (int i = 0; i < neighbourGOs.Count; i++)
        //    {
        //        GridTile nextNTile = neighbourGOs[i].GetComponent<GridTile>();
        //        nextNTile.SwapTile(TileState.Dirt, false);
        //    }
        //}
    }

    public int GetColumn()
    {
        return (int) _coordinates.x;
    }

    public int GetRow()
    {
        return (int)_coordinates.y;
    }

    public Vector2 GetCoordinates()
    {
        return _coordinates;
    }

}


public struct TileStateHistory
{
    public Vector2 coordinates;
    public GridTile.TileState startState;
    public GridTile.TileState endState;

    public TileStateHistory(Vector2 coords, GridTile.TileState start, GridTile.TileState end)
    {
        coordinates = coords;
        startState  = start;
        endState    = end;
    }
}