using UnityEngine;
using System.Collections.Generic;

public class BenchTile : MonoBehaviour
{
    private GridTile tileRef;

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

             if ( neighbours[1] != null && neighbours[1].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta = -60 * 2; } //  S
        else if ( neighbours[2] != null && neighbours[2].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta = -60 * 3; } // SE
        else if ( neighbours[3] != null && neighbours[3].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta = -60 * 1; } // SW
        else if ( neighbours[4] != null && neighbours[4].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta =  60 * 2; } // NE
        else if ( neighbours[5] != null && neighbours[5].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta =  60 * 0; } // NW
        else if ( neighbours[0] != null && neighbours[0].GetComponent<GridTile>().state == GridTile.TileState.Dirt) { rDelta =  60 * 1; } //  N

        transform.localEulerAngles = new Vector3(0, 0, rDelta);
    }
}
