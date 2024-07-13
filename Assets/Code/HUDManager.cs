using UnityEngine;

public class HUDManager : MonoBehaviour
{
    static public GridTile.TileState selectedType;

    void Start()
    {   
        selectedType = GridTile.TileState.Bush;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1)) selectedType = GridTile.TileState.Grass;
        if (Input.GetKey(KeyCode.Alpha2)) selectedType = GridTile.TileState.Dirt ;
        if (Input.GetKey(KeyCode.Alpha3)) selectedType = GridTile.TileState.Tree ;
        if (Input.GetKey(KeyCode.Alpha4)) selectedType = GridTile.TileState.Bush ;
        if (Input.GetKey(KeyCode.Alpha5)) selectedType = GridTile.TileState.Rock ;
        if (Input.GetKey(KeyCode.Alpha6)) selectedType = GridTile.TileState.Water;
        if (Input.GetKey(KeyCode.Alpha0)) selectedType = GridTile.TileState.Base ;
    }

    public void SelectTileType(int tile)
    {
        selectedType = (GridTile.TileState) tile;
    }
}