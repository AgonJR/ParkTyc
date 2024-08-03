using UnityEngine;
// using UnityEditor;
using System.Collections.Generic;
using FMODUnity;
using System;

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
    public GameObject tileBench;
    public GameObject tileCamp;

    [Header("Tile SFX")]
    public EventReference SFXBase ;
    public EventReference SFXGrass;
    public EventReference SFXDirt ;
    public EventReference SFXTree ;
    public EventReference SFXBush ;
    public EventReference SFXRock ;
    public EventReference SFXWater;
    public EventReference SFXBench;
    public EventReference SFXRotate;

    public enum TileState
    {
        Base,
        Grass,
        Dirt,
        Tree,
        Bush,
        Rock,
        Water,
        Bench,
        Camp
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
        { TileState.Bench,-1 },
        { TileState.Camp, -1 }
    };

    public static Dictionary<TileState, int> stateUnlockCost = new()
    {
        { TileState.Base,   0 },
        { TileState.Grass,  0 },
        { TileState.Dirt,   0 },
        { TileState.Tree,  15 },
        { TileState.Bush,   5 },
        { TileState.Rock,  10 },
        { TileState.Water,  5 },
        { TileState.Bench, 25 },
        { TileState.Camp,  50 }
    };

    [Header("Tile Status")]
    public TileState state  = TileState.Base;
    [Space]
    public bool isActivity  = false;
    public int maxCapacity  = 0;
    public int curOccupancy = 0;

    private Vector2 _coordinates;
    private Vector3 _highlightPos;
    private GameObject _activeTileGO;
    private GridTile[] _neighbourTiles;
    private List<NPCBrain> _activeNPCs = new();

    [HideInInspector]
    public int ditherFlags = 0; 
    private bool dithered = false;

    private static GameObject _highlightTileObject;

    void Awake()
    {
        if (_highlightTileObject == null)
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

        HighlighterGhosts.instance.HideIfMatch(state);
    }

    void OnMouseExit()
    {
        if (Input.GetMouseButton(0) == false)
        {
            _highlightTileObject.SetActive(false);
        }

        Dither(false);
        DitherNeighbours(false);
    }

    private void OnMouseDown()
    {
        ToggleTileState();
    }

    private void ToggleTileState()
    {
        SwapTile(HUDManager.selectedType);
    }

    private void OnMouseOver()
    {
        if ( Input.GetKeyDown(KeyCode.R ) )
        {
            switch (state)
            {
                case TileState.Dirt:  return;
                case TileState.Base:  return;
                case TileState.Water: return;
                case TileState.Grass: return;
            }
            
            RotateTile(1);
        }

        Dither(Input.GetKey(KeyCode.LeftShift));
        DitherNeighbours(Input.GetKey(KeyCode.LeftShift));
    }

    public void GatherNeighbourRefs(bool force = false)
    {
        if ( force ) _neighbourTiles = null;

        if ( _neighbourTiles == null || _neighbourTiles.Length == 0 )
        {
            _neighbourTiles = new GridTile[6];

            List<GameObject> neighbourGOs = GridManager.instance.GetNeighbouringTilesConst(GetColumn(), GetRow());

            for ( int i = 0; i < 6; i++ )
            {
                _neighbourTiles[i] = neighbourGOs[i] == null ? null : neighbourGOs[i].GetComponent<GridTile>();
            }
        }
    }

    public void DitherNeighbours(bool toggle)
    {
        GatherNeighbourRefs();

        for ( int i = 0; i < 6; i++ )
        {
            if ( _neighbourTiles[i] != null ) _neighbourTiles[i].Dither(toggle);
        }
    }

    public void SwapTile(GridTile.TileState targetState, bool addToUndo = true)
    {
        if ( state == targetState ) return;
        if ( addToUndo && GameManager.Score < stateUnlockCost[targetState] ) return;

        if ( addToUndo ) GridManager.AddToUndoHistory(new TileStateHistory(_coordinates, state, targetState));

        // Update Score
        int sDelta = addToUndo ? -1 : 1;
        sDelta *= stateUnlockCost[addToUndo ? targetState : state]; // Refund current state if undo
        GameManager.instance.AddToScore(sDelta);

        // Set State & Toggle Tile
        state = targetState;

        if (tileBase  != null)  tileBase.SetActive(TileState.Base  == state);
        if (tileGrass != null) tileGrass.SetActive(TileState.Grass == state);
        if (tileDirt  != null)  tileDirt.SetActive(TileState.Dirt  == state);
        if (tileTree  != null)  tileTree.SetActive(TileState.Tree  == state);
        if (tileBush  != null)  tileBush.SetActive(TileState.Bush  == state);
        if (tileRock  != null)  tileRock.SetActive(TileState.Rock  == state);
        if (tileWater != null) tileWater.SetActive(TileState.Water == state);
        if (tileBench != null) tileBench.SetActive(TileState.Bench == state);
        if (tileCamp  != null)  tileCamp.SetActive(TileState.Camp  == state);

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
            case TileState.Rock : isActivity = true; maxCapacity = 1; curOccupancy = 0; break;
            default: isActivity = false; break;
        }

        ClearOccupants();
        InitializeActiveTile();
        HighlighterGhosts.instance.HideIfMatch(state);
        
        if ( addToUndo ) PingNeighbours();
        if ( addToUndo ) PingObjectives();
        if ( addToUndo ) NPCManager.ForceScan();
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

    public void RotateTile(int times = 1)
    {
        FMODUnity.RuntimeManager.PlayOneShot(SFXRotate);
        if ( _activeTileGO != null )
        {
            if ( state == TileState.Camp )
            {
                if ( GetComponentInChildren<CampTile>().IsComplete() || GetComponentInChildren<CampTile>().IsHalfComplete()) return;
            }

            Transform tileT = _activeTileGO.transform;
            tileT.localEulerAngles = new Vector3(tileT.localEulerAngles.x, tileT.localEulerAngles.y, tileT.localEulerAngles.z + (60 * times));

            if ( state == TileState.Bench )
            {
                GetComponentInChildren<BenchTile>().TurnSitRotation(times);
                if ( curOccupancy > 0 ) { foreach(NPCBrain npc in _activeNPCs) { npc.RotateOnBench(); } }
            }

            return;
        }

        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z + (60 * times));
        
    }

    public void InitializeActiveTile()
    {
        switch (state)
        {
            case TileState.Base:  _activeTileGO = tileBase;  break;
            case TileState.Grass: _activeTileGO = tileGrass; break;
            case TileState.Dirt:  _activeTileGO = tileDirt;  break;
            case TileState.Tree:  _activeTileGO = tileTree;  break;
            case TileState.Bush:  _activeTileGO = tileBush;  break;
            case TileState.Rock:  _activeTileGO = tileRock;  break;
            case TileState.Water: _activeTileGO = tileWater; break;
            case TileState.Bench: _activeTileGO = tileBench; break;
            case TileState.Camp:  _activeTileGO = tileCamp;  break;
        } 

        if ( state == TileState.Bench ) 
        {
            _activeTileGO.GetComponent<BenchTile>().Initialize(this);
        }

        if ( state == TileState.Camp ) 
        {
            _activeTileGO.GetComponent<CampTile>().Initialize(this);
        }
    }

    public void PingNeighbours()
    {
        GatherNeighbourRefs();

        for ( int i = 0; i < 6; i++ )
        {
            if ( _neighbourTiles[i] != null ) _neighbourTiles[i].PingActiveTile();
        }
    }

    public void PingObjectives()
    {
        ObjectiveSystem.Ping_TileBuilt(state);
    }

    public void PingActiveTile()
    {
        if ( state == TileState.Camp ) 
        {
            _activeTileGO.GetComponent<CampTile>().RecalculateStatus();
        }
    }

    public void Dither(bool toggle)
    {
        if ( dithered == toggle )
        {
            return;
        }

        if ( (ditherFlags & (1 << (int)state)) != 0 )
        {
            MeshRenderer[] ditherMeshes = _activeTileGO.GetComponentsInChildren<MeshRenderer>();
            List<Material> ditherMaterials = new();
            Array.ForEach(ditherMeshes, mesh => ditherMaterials.AddRange(mesh.materials));

            for (int i = 0; i < ditherMaterials.Count; i++)
            {
                ditherMaterials[i].SetFloat("_Dither", toggle ? 1 : 0);
            }

            for (int i = 0; i < ditherMeshes.Length; i++)
            {
                ditherMeshes[i].shadowCastingMode = toggle ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        dithered = toggle;
    }

    public void Occupy(NPCBrain npc)
    {
        curOccupancy++;
        _activeNPCs.Add(npc);
    }

    public void UnOccupy(NPCBrain npc)
    {
        curOccupancy--;
        _activeNPCs.Remove(npc);
    }

    public void ClearOccupants()
    {
        if (_activeNPCs != null && _activeNPCs.Count > 0)
        {
            foreach (NPCBrain npc in _activeNPCs) { npc.EndActivity(); }
            _activeNPCs.Clear();
            curOccupancy = 0;
        }
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

// [CustomEditor(typeof(GridTile))]
// public class GridTileInspector : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();

//         GridTile GT = (GridTile) target;

//         GUILayout.Space(10);
//         GT.ditherFlags = EditorGUILayout.MaskField("Dither Tiles", GT.ditherFlags, Enum.GetNames(typeof(GridTile.TileState)));

//         GUILayout.Space(10);
//         GUILayout.Label("Unlock Costs", EditorStyles.boldLabel);

//         for ( int i = 1; i < GridTile.stateUnlockCost.Count; i++)
//         {
//             GridTile.TileState tileState = (GridTile.TileState) i;

//             GUILayout.BeginHorizontal();
//                 GUILayout.Label(tileState.ToString(), GUILayout.MinWidth(100));
//                 GridTile.stateUnlockCost[tileState] = EditorGUILayout.IntField(GridTile.stateUnlockCost[tileState]);
//             GUILayout.EndHorizontal();
//         }

//         EditorUtility.SetDirty(GT);
//     }
// }