using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTile : MonoBehaviour
{
    public GameObject tileDirt;
    public GameObject tileGrass;
    public GameObject tilePathCurveSharp;
    public GameObject tilePathCurveSmooth;
    public GameObject tilePathStraight;
    public GameObject tilePathStraightEnd;
    public GameObject tilePathChunkCenter;
    public GameObject tilePathChunkEdgeA;
    public GameObject tilePathChunkEdgeB;

    // Start is called before the first frame update
    void Start()
    {
        GridTile thisGridTile = gameObject.GetComponentInParent<GridTile>();
        List<GameObject> neighbouringTileGO = GridManager.instance.GetNeighbouringTilesConst(thisGridTile.GetColumn(), thisGridTile.GetRow());
        
        // Configurations
        // 1. Solid Path : Every neighbour is path
        // 2. Line Path : Only two neighbours are dirt, and they are not neighbours to each other
        // 3. Blob Path : Brute force edge connections until every edge corner is filled (12 in total)

        // Solid Path
        if (true) {
            SetupSolidPath();
        }
        // Line Path
        else if (true) {
            SetupLinePath();   
        }
        // Blob Path
        else if (true) {
            SetupBlobPath();
        }
    }

    void SetupSolidPath()
    {
        Debug.Log("Path Tile: Setup solid path");
    }

    void SetupLinePath()
    {
        Debug.Log("Path Tile: Setup line path");
    }

    void SetupBlobPath()
    {
        Debug.Log("Path Tile: Setup blob path");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
