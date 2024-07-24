using UnityEngine;
using System.Collections.Generic;

public class CampTile : MonoBehaviour
{
    public GameObject campIncomplete;
    public GameObject campComplete;

    private GridTile tileRef;
    
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

        campIncomplete.SetActive(true);
    }

}