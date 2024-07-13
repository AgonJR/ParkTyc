using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{

    // public enum TileState
    // {
    // Base,
    // Grass,
    // Dirt,
    // Tree,
    // Bush,
    // Rock,
    // Water
    // }

    // [Header("Data")]
    static public GridTile.TileState selectedType;

    // Start is called before the first frame update
    void Start()
    {   
        selectedType = GridTile.TileState.Base;
        Debug.Log(selectedType);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SelectTileType(int tile)
    {
        selectedType = (GridTile.TileState)tile;
        Debug.Log(selectedType);
        Debug.Log(tile);
    }
}