using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTile : MonoBehaviour
{
    public GameObject tileGrass;
    public GameObject tileSolid;
    [Space]
    public GameObject tilePathCurveSharp;
    public GameObject tilePathCurveSmooth;
    public GameObject tilePathStraight;
    public GameObject tilePathEnd;
    [Space]
    public GameObject tileChunkCenter;
    public GameObject tileChunkEdgeEnd;
    public GameObject tileChunkEdgeContinue;

    private int q;
    private int r;

    private List<GridTile> neighbourTiles = new List<GridTile>();
    private int n; // neighbour count

    private List<PathTile> neighbourPaths = new List<PathTile>();
    private int d; // dirt neighbour count



    private void Awake()
    {
        // Get Coordinates
        GridTile tile = GetComponentInParent<GridTile>();
        q = tile.GetColumn();
        r = tile.GetRow();
        n = 0;

        // Get Neighbour Tile References
        foreach (GameObject nGo in GridManager.instance.GetNeighbouringTilesConst(q, r))
        {
            n += nGo == null ? 0 : 1;
            neighbourTiles.Add(nGo != null ? nGo.GetComponent<GridTile>() : null );
        }

        RecalculateShape(true);
    }

    /*

    Configurations:

        Notes: 
            - Water is considered Grass/Water type, so dirt tiles should not blend into it. 
              Later on Water tiles will have a grassy shoreline that completes the connection.

                    1. Solid Dirt -> Every neighbour is Dirt
                        - Use tileSolid and change material to altMaterial
                        - Hide all tileChunk types
                        - Hide all tilePath types

                    2. Path End -> Single Dirt neighbour.
                        - Use tileSolid                
                        - Hide all tileChunk types
                        - Use tilePathEnd and hide all other path types

                    3. Line Path -> Two neighbours are Dirt type and those neighbours are not adjacent to one another
                        - Use tileSolid
                        - Hide all tileChunk types
                        - Use either tilePathStraight or tilePathCurveSmooth and rotate the tile to finish connection (flipping the tile will work but shouldn't be nessecary)
                        - Hide all remaining tilePath types

        4. Dirt Area -> Brute force edge connections until every edge chunk is filled (2 chunks per edge (L, R), 12 chunks in total)
            - Hide tileSolid
            - 2 Chunks per side (left and right, left being a copy of right that's been mirrored on the x axis)
            - use tileChunkEdgeEnd when two adjacent neighbour tiles are of different types (Grass/Water type or Dirt type)
            - use tileChunkEdgeContinue when both tiles are Grass/Water types
            - use use tileChunkEdgeEnd or tileChunkEdgeContinue when both tiles are Dirt types (change material to altMaterial, will work for either tile)
    */


    void OnEnable()
    {
        RecalculateShape(true);
    }

    public void RecalculateShape(bool pingNeighbours = false)
    {
        d = 0; // Count Neighbour Dirt Tiles
        neighbourPaths.Clear();
        foreach (GridTile nT in neighbourTiles)
        {
            if (nT != null && nT.state == GridTile.TileState.Dirt)
            {
                d++;
                neighbourPaths.Add(nT.GetComponentInChildren<PathTile>());
            }
            else
            {
                neighbourPaths.Add(null);
            }
        }


        if (d == n)
        {
            ModifyPath_Solid();
        }
        else
        {
            switch (d) // Modify
            {
                case 0: ModifyPath_Center();     break;
                case 1: ModifyPath_End();        break;
                case 2: ModifyPath_ConnectTwo(); break;
                case 3: ModifyPath_Solid();      break;
                case 4: ModifyPath_Solid();      break;
                case 5: ModifyPath_Solid();      break;
                case 6: ModifyPath_Solid();      break;
            }
        }


        if (pingNeighbours) // Modify Neighbours 
        {
            foreach (PathTile pT in neighbourPaths)
            {
                if (pT != null) pT.RecalculateShape(false);
            }
        }
    }

    private void OnDisable()
    {
        foreach (PathTile pT in neighbourPaths)
        {
            if (pT != null) pT.RecalculateShape(false);
        }
    }


    private void ModifyPath_Center()
    {
        ToggleAllSubTiles(false);
        tileGrass.SetActive(true);
        tileChunkCenter.SetActive(true);
    }

    private void ModifyPath_Solid()
    {
        ToggleAllSubTiles(false);
        tileSolid.SetActive(true);
    }

    private void ModifyPath_End()
    {
        ToggleAllSubTiles(false);
        tileGrass.SetActive(true);
        tilePathEnd.SetActive(true);

        //
        // Rotate - - - 0 N, 1 S, 2 SE, 3 SW, 4 NE, 5 NW - - -
        //

        for (int i = 0; i < neighbourPaths.Count; i++)
        {
            if (neighbourPaths[i] != null)
            {
                float r = 60.0f;

                switch (i)
                {
                    case 0: r *= 0; break;
                    case 1: r *= 3; break;
                    case 2: r *= 2; break;
                    case 3: r *= 4; break;
                    case 4: r *= 1; break;
                    case 5: r *= 5; break;
                }

                transform.localEulerAngles = new Vector3(0, 0, r);

                break;
            }
        }
    }

    private void ModifyPath_ConnectTwo()
    {
        ToggleAllSubTiles(false);

        bool firstFound = false;

        int i1 = -1;
        int i2 = -1;
        int q1 = -1;
        int q2 = -1;
        int r1 = -1;
        int r2 = -1;

        for ( int i = 0; i < 6; i++)
        {
            PathTile pT = neighbourPaths[i];

            if (pT != null)
            {
                if (firstFound)
                {
                    i2 = i;
                    q2 = pT.q;
                    r2 = pT.r;
                }
                else
                {
                    i1 = i;
                    q1 = pT.q;
                    r1 = pT.r;
                    firstFound = true;
                }
            }
        }

        // Check if adjascent
        bool adjacent = false;
        int  adjCombo = -1;

             if ((i1 == 0 || i1 == 4) && (i2 == 0 || i2 == 4)) { adjacent = true; adjCombo = 1; } // NE & N
        else if ((i1 == 2 || i1 == 4) && (i2 == 2 || i2 == 4)) { adjacent = true; adjCombo = 2; } // NE & SE
        else if ((i1 == 2 || i1 == 1) && (i2 == 2 || i2 == 1)) { adjacent = true; adjCombo = 3; } // SE & S
        else if ((i1 == 3 || i1 == 1) && (i2 == 3 || i2 == 1)) { adjacent = true; adjCombo = 4; } // SW & S
        else if ((i1 == 3 || i1 == 5) && (i2 == 3 || i2 == 5)) { adjacent = true; adjCombo = 5; } // SW & NW
        else if ((i1 == 0 || i1 == 5) && (i2 == 0 || i2 == 5)) { adjacent = true; adjCombo = 0; } // NW & N

        if (adjacent)
        {
            tileGrass.SetActive(true);
            tilePathCurveSharp.SetActive(true);
            transform.localEulerAngles = new Vector3(0, 0, 60 * adjCombo);
        }
        else
        {
            // Check for smooth corners\
            bool  corner = false;
            int crnCombo = -1;

                 if ((i1 == 0 || i1 == 3) && (i2 == 0 || i2 == 3)) { corner = true; crnCombo = 0; } // SW & N
            else if ((i1 == 5 || i1 == 1) && (i2 == 5 || i2 == 1)) { corner = true; crnCombo = 1; } // NW & S
            else if ((i1 == 2 || i1 == 3) && (i2 == 2 || i2 == 3)) { corner = true; crnCombo = 2; } // SE & SW
            else if ((i1 == 4 || i1 == 1) && (i2 == 4 || i2 == 1)) { corner = true; crnCombo = 3; } // NE & S
            else if ((i1 == 2 || i1 == 0) && (i2 == 2 || i2 == 0)) { corner = true; crnCombo = 4; } // SE & N
            else if ((i1 == 4 || i1 == 5) && (i2 == 4 || i2 == 5)) { corner = true; crnCombo = 5; } // NW & NE

            if (corner)
            {
                tileGrass.SetActive(true);
                tilePathCurveSmooth.SetActive(true);
                transform.localEulerAngles = new Vector3(0, 0, -60 * crnCombo);
            }
            else if ((q1 == q2 && Mathf.Abs(r1 - r2) == 2) || // vertical opposites
            (Mathf.Abs(q1 - q2) == 2 && Mathf.Abs(r1 - r2) == 1)) // diagonal opposites
            {
                tileGrass.SetActive(true);
                tilePathStraight.SetActive(true);

                if ((i1 == 3 || i1 == 4) && (i2 == 3 || i2 == 4)) // NE & SW
                {
                    transform.localEulerAngles = new Vector3(0, 0, 60);
                }
                else if ((i1 == 5 || i1 == 2) && (i2 == 5 || i2 == 2)) // NW & SE
                {
                    transform.localEulerAngles = new Vector3(0, 0, 120);
                }
            }
            else
            {
                tileSolid.SetActive(true);
            }
        }
    }

    private void ToggleAllSubTiles(bool enable)
    {
        tileGrass.SetActive(enable);
        tileSolid.SetActive(enable);

        tilePathCurveSharp.SetActive(enable);
        tilePathCurveSmooth.SetActive(enable);
        tilePathStraight.SetActive(enable);
        tilePathEnd.SetActive(enable);

        tileChunkCenter.SetActive(enable);
        tileChunkEdgeEnd.SetActive(enable);
        tileChunkEdgeContinue.SetActive(enable);
    }

}
