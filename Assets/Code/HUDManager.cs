using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    static public GridTile.TileState selectedType;

    public Color defaultColour;
    public Color selectdColour;
    [Space]
    public Button[] tileButts;


    void Start()
    {
        SelectTileType(GridTile.TileState.Bush);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1)) SelectTileType(GridTile.TileState.Grass);
        if (Input.GetKey(KeyCode.Alpha2)) SelectTileType(GridTile.TileState.Dirt );
        if (Input.GetKey(KeyCode.Alpha3)) SelectTileType(GridTile.TileState.Tree );
        if (Input.GetKey(KeyCode.Alpha4)) SelectTileType(GridTile.TileState.Bush );
        if (Input.GetKey(KeyCode.Alpha5)) SelectTileType(GridTile.TileState.Rock );
        if (Input.GetKey(KeyCode.Alpha6)) SelectTileType(GridTile.TileState.Water);
        if (Input.GetKey(KeyCode.Alpha0)) SelectTileType(GridTile.TileState.Base );
    }

    public void SelectTileType(GridTile.TileState newType)
    {
        SelectTileType((int)newType);
    }

    public void SelectTileType(int tile)
    {
        selectedType = (GridTile.TileState) tile;

        for (int i = 0; i < tileButts.Length; i++)
        {
            var colors = tileButts[i].colors;
            colors.normalColor = i + 1 == (int)selectedType ? selectdColour : defaultColour;
            tileButts[i].colors = colors;
        }
    }
}