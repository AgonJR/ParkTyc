using UnityEngine;
using System.Collections.Generic;
using FMODUnity;

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
    [Space]
    public GameObject tileBench;

    [Header("Tile SFX")]
    public EventReference SFXBase ;
    public EventReference SFXGrass;
    public EventReference SFXDirt ;
    public EventReference SFXTree ;
    public EventReference SFXBush ;
    public EventReference SFXRock ;
    public EventReference SFXWater;
    [Space]
    public EventReference SFXBench;

    public enum TileState
    {
        Base,
        Grass,
        Dirt,
        Tree,
        Bush,
        Rock,
        Water,
        Bench
    }

    public static readonly Dictionary<TileState, int> stateWalkScores = new()
    {
        { TileState.Base,  9 },
        { TileState.Grass, 5 },
        { TileState.Dirt,  1 },
        { TileState.Tree, -1 },
        { TileState.Bush, -1 },
        { TileState.Rock, -1 },
        { TileState.Water,-1 },
        { TileState.Bench,-1 }
    };

    public static readonly Dictionary<TileState, int> stateUnlockCost = new()
    {
        { TileState.Base,  0  },
        { TileState.Grass, 0  },
        { TileState.Dirt,  0  },
        { TileState.Tree,  5  },
        { TileState.Bush,  0  },
        { TileState.Rock,  20 },
        { TileState.Water, 35 },
        { TileState.Bench, 50 }
    };


    [Header("Tile Status")]
    public TileState state  = TileState.Base;
    [Space]
    public bool isActivity  = false;
    public int maxCapacity  = 0;
    public int curOccupancy = 0;


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

        // Set State
        if (tileBase  != null)  tileBase.SetActive(TileState.Base  == state);
        if (tileGrass != null) tileGrass.SetActive(TileState.Grass == state);
        if (tileDirt  != null)  tileDirt.SetActive(TileState.Dirt  == state);
        if (tileTree  != null)  tileTree.SetActive(TileState.Tree  == state);
        if (tileBush  != null)  tileBush.SetActive(TileState.Bush  == state);
        if (tileRock  != null)  tileRock.SetActive(TileState.Rock  == state);
        if (tileWater != null) tileWater.SetActive(TileState.Water == state);
        if (tileBench != null) tileBench.SetActive(TileState.Bench == state);

        // Play SFX if NOT Undo
        if (addToUndo)
        {
            switch (state)
            {
                case TileState.Base:  RuntimeManager.PlayOneShot(SFXBase,  transform.position); break;
                case TileState.Grass: RuntimeManager.PlayOneShot(SFXGrass, transform.position); break;
                case TileState.Dirt:  RuntimeManager.PlayOneShot(SFXDirt,  transform.position); break;
                case TileState.Tree:  RuntimeManager.PlayOneShot(SFXTree,  transform.position); break;
                case TileState.Bush:  RuntimeManager.PlayOneShot(SFXBush,  transform.position); break;
                case TileState.Rock:  RuntimeManager.PlayOneShot(SFXRock,  transform.position); break;
                case TileState.Water: RuntimeManager.PlayOneShot(SFXWater, transform.position); break;
                case TileState.Bench: RuntimeManager.PlayOneShot(SFXBench, transform.position); break;
            }   
        }

        // Interactable Tiles
        switch ( state )
        {
            case TileState.Bench: isActivity = true; maxCapacity = 1; curOccupancy = 0; break;
            default: isActivity = false; break;
        }
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