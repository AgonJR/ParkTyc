using System.Collections.Generic;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    [Range(0, 30)] public float npcSpeed = 5.0f;
    [Space]
    public GameObject nextTarget;
    public float minTrgtDistance;

    private int[,] _gridVisited;
    private Vector2 _coordinates;
    private GameObject _exitTarget;
    private Vector2 _exitCoordinates;

    public void Init(int spawnQ, int spawnR)
    {
        gameObject.name = "N P C  (" + spawnQ + " , " + spawnR + ")";

        int gridSize = GridManager.instance.gridSize;

        _coordinates = new Vector2(spawnQ, spawnR);

        _gridVisited = new int[gridSize, gridSize];


        for (int q = 0; q < gridSize; q++) //column
        {
            for (int r = 0; r < gridSize; r++) //row
            {
                _gridVisited[q, r] = 0;
            }
        }


        _gridVisited[spawnQ, spawnR] = 1;
    }

    private void FixedUpdate()
    {
        ProcessMovement();
    }

    private void ProcessMovement()
    {
        if (nextTarget == null)
        {
            SelectNextTargetTile();
        }
        else
        {
            // Check If Target Reached
            float distance = Vector3.Distance(transform.position, nextTarget.transform.position);

            if (distance < minTrgtDistance)
            {
                GridTile targetTile = nextTarget.GetComponent<GridTile>();

                int q = targetTile.GetColumn();
                int r = targetTile.GetRow();

                gameObject.name = "N P C  (" + q + " , " + r + ")";

                _coordinates = new Vector2(q, r);

                if (_exitCoordinates == _coordinates)
                {
                    Debug.Log(" --- ! N P C ! Exit ! +5 Points !");
                    Destroy(gameObject);
                }

                _gridVisited[q, r]++;

                nextTarget = null;

                return;
            }

            // Move Forwards Target
            Vector3 direction = nextTarget.transform.position - transform.position;

            direction.y = 0.0f;

            transform.position += direction.normalized * npcSpeed * Time.deltaTime;
        }
    }

    private void SelectNextTargetTile()
    {
        List<GameObject> neighbourGOs = GridManager.instance.GetNeighbouringTiles((int)_coordinates.x, (int)_coordinates.y);
        List<GridTile> neighbourTiles = new List<GridTile>();

        int[] nWeights = new int[neighbourGOs.Count];

        for ( int i = 0; i < neighbourGOs.Count; i++ )
        {
            GridTile nextNTile = neighbourGOs[i].GetComponent<GridTile>();
            neighbourTiles.Add(nextNTile);

            nWeights[i] = 1 + _gridVisited[nextNTile.GetColumn(), nextNTile.GetRow()];
            nWeights[i] *= GridTile.stateWalkScores[nextNTile.state];
        }

        int minWeight = int.MaxValue;


        for (int i = 0; i < neighbourGOs.Count; i++)
        {
            if (nWeights[i] < 0)
            {
                continue;
            }

            if (nWeights[i] < minWeight)
            {
                minWeight = nWeights[i];
                nextTarget = neighbourGOs[i];
            }
        }
    }

    public void SelectExitTarget(List<GameObject> exitTiles)
    {
        if (_exitTarget == null)
        {
            _exitTarget = exitTiles[(int)Random.Range(0, exitTiles.Count)];
        }

        _exitCoordinates = _exitTarget.GetComponent<GridTile>().GetCoordinates();
    }
}