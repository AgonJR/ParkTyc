using UnityEngine;
using System.Collections.Generic;

public class BenchTile : MonoBehaviour
{
    public Transform actMark;
    private GridTile tileRef;

    private int _sitRotation = 0;

    public void Initialize(GridTile tile)
    {
        tileRef = tile;
        FacePath();
    }

    private void FacePath()
    {
        // This could be much smarter like the Path Tile

        List<GameObject> neighbours = GridManager.instance.GetNeighbouringTilesConst(tileRef.GetColumn(), tileRef.GetRow());

        int rDelta = -120;

             if ( neighbours[1] != null && neighbours[1].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta = -60 * 2; _sitRotation = 60 * 3; } //  S
        else if ( neighbours[2] != null && neighbours[2].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta = -60 * 3; _sitRotation = 60 * 2; } // SE
        else if ( neighbours[3] != null && neighbours[3].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta = -60 * 1; _sitRotation = 60 * 4; } // SW
        else if ( neighbours[4] != null && neighbours[4].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta =  60 * 2; _sitRotation = 60 * 1; } // NE
        else if ( neighbours[5] != null && neighbours[5].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta =  60 * 0; _sitRotation = 60 * 5; } // NW
        else if ( neighbours[0] != null && neighbours[0].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta =  60 * 1; _sitRotation = 60 * 0; } //  N

        transform.localEulerAngles = new Vector3(0, 0, rDelta);
    }

    public int getSitRotation()
    {
        return _sitRotation;
    }

    public void TurnSitRotation(int times) // Called when the tile is rotated
    {
        _sitRotation += 60 * times;
    }
}
