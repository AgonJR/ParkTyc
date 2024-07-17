using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTile : MonoBehaviour
{
    public GameObject tileSolid;
    public GameObject tilePathCurveSharp;
    public GameObject tilePathCurveSmooth;
    public GameObject tilePathStraight;
    public GameObject tilePathEnd;
    public GameObject tileChunkCenter;
    public GameObject tileChunkEdgeEnd;
    public GameObject tileChunkEdgeContinue;
    public Material altMaterial;

    // Start is called before the first frame update
    void Start()
    {        
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
