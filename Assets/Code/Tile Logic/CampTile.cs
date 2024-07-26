using UnityEngine;
using System.Collections.Generic;

public class CampTile : MonoBehaviour
{
    public GameObject campIncomplete;
    public GameObject campComplete;
    [Space]
    public GameObject ghostTree;
    public GameObject ghostRock;

    private GridTile tileRef;

    private GameObject subtileA;
    private GameObject subtileB;
    
    public void Initialize(GridTile tile)
    {
        tileRef = tile;

        campIncomplete.SetActive(false);
        campComplete.SetActive(false);

        RecalculateStatus();
    }

    public void RecalculateStatus()
    {
        List<GameObject> neighbours = GridManager.instance.GetNeighbouringTilesConst(tileRef.GetColumn(), tileRef.GetRow());

        GridTile[] nTiles = new GridTile[6];
        for (int i = 0; i < 6; i++) { nTiles[i] = neighbours[i] == null ? null : neighbours[i].GetComponent<GridTile>(); }

        int[] clockwiseCardinals = {0, 4, 2, 1, 3, 5, 0};  // N, NE, SE, S, SW, NW, N

        // Check if both Tree & Rock already present
        for (int i = 0; i < 6; i++)
        {
            int a = clockwiseCardinals[i];
            int b = clockwiseCardinals[i+1];

            if ( nTiles[a] == null || nTiles[b] == null ) { continue; }
            if (( nTiles[a].state == GridTile.TileState.Rock && nTiles[b].state == GridTile.TileState.Tree )
             || ( nTiles[a].state == GridTile.TileState.Tree && nTiles[b].state == GridTile.TileState.Rock ))
            {

                campComplete.SetActive(true);
                campComplete.transform.localEulerAngles = new Vector3(0, 0, 60 * (i+3));

                subtileA = nTiles[a].gameObject;
                subtileB = nTiles[b].gameObject;

                campIncomplete.SetActive(false);
                nTiles[a].gameObject.SetActive(false);
                nTiles[b].gameObject.SetActive(false);

                return;
            }
        }

        //Check if only Tree OR Rock already present
        for (int i = 0; i < 6; i++)
        {
            int a = clockwiseCardinals[i];

            if ( nTiles[a] == null ) { continue; }
            if ( nTiles[a].state == GridTile.TileState.Rock )
            {
                campIncomplete.SetActive(true);
                campIncomplete.transform.localEulerAngles = new Vector3(0, 0, 60 * (i+3));

                subtileA = nTiles[a].gameObject;

                ghostTree.SetActive(true);
                ghostRock.SetActive(false);

                return;
            }
            if ( nTiles[a].state == GridTile.TileState.Tree )
            {
                campIncomplete.SetActive(true);
                campIncomplete.transform.localEulerAngles = new Vector3(0, 0, 60 * (i+2));

                subtileB = nTiles[a].gameObject;

                ghostTree.SetActive(false);
                ghostRock.SetActive(true);

                return;
            }
        }

        // No components found, spawn both construction tiles
        if ( subtileA != null ) subtileA.SetActive(true);
        if ( subtileB != null ) subtileB.SetActive(true);

        ghostRock.SetActive(true);
        ghostTree.SetActive(true);
        campComplete.SetActive(false);
        campIncomplete.SetActive(true);

        // Rotate for edge tile camp placement
        bool edgeCheck = false;
        foreach (GridTile tile in nTiles) { if (tile == null) {edgeCheck = true; break;}}
        if ( edgeCheck )
        {
            for (int i = 0; i < 6; i++)
            {
                int a = clockwiseCardinals[i];
                int b = clockwiseCardinals[i+1];

                if ( nTiles[a] != null && nTiles[b] != null )
                {
                    campIncomplete.transform.localEulerAngles = new Vector3(0, 0, 60 * (i+3));
                    return;
                }
            }
        }

    }

    private void OnDisable()
    {
        if ( subtileA != null ) subtileA.SetActive(true);
        if ( subtileB != null ) subtileB.SetActive(true);
    }

    public bool IsComplete()
    {
        return campComplete.activeInHierarchy;
    }

    public bool IsHalfComplete()
    {
        return subtileA != null || subtileB != null;
    }

}