using UnityEngine;

public class HUDManager : MonoBehaviour
{
    static public GridTile.TileState selectedType;

    void Start()
    {   
        selectedType = GridTile.TileState.Bush;

        //Debug.Log(selectedType);
    }

    public void SelectTileType(int tile)
    {
        selectedType = (GridTile.TileState) tile;

        //Debug.Log(selectedType);
        //Debug.Log(tile);
    }
}