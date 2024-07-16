using System.Collections.Generic;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    [Range(0, 30)] public float npcSpeed = 5.0f;
    [Space]
    public GameObject nextTarget;
    public float minTrgtDistance;

    [SerializeField] private int[,] _gridVisited;
    [SerializeField] private float[,] _exitHeurstx;

    private Vector2 _coordinates;
    [SerializeField] private GameObject _exitTarget;
    private Vector2 _exitCoordinates;

    public void Init(int spawnQ, int spawnR)
    {
        gameObject.name = "N P C  (" + spawnQ + " , " + spawnR + ")";

        int gridSize = GridManager.instance.gridSize;

        _coordinates = new Vector2(spawnQ, spawnR);

        _gridVisited = new int[gridSize, gridSize];
        _exitHeurstx = new float[gridSize, gridSize];


        for (int q = 0; q < gridSize; q++) //column
        {
            for (int r = 0; r < gridSize; r++) //row
            {
                _gridVisited[q, r] = 0;
                _exitHeurstx[q, r] = 1;
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
        if (_exitTarget == null)
        {
            SelectExitTarget(NPCManager.RequestExitTiles());
        }

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
                    GameManager.instance.AddToScore(5);
                    NPCManager.instance.ClearNPC(gameObject);
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

        float[] nWeights = new float[neighbourGOs.Count];

        for ( int i = 0; i < neighbourGOs.Count; i++ )
        {
            GridTile nextNTile = neighbourGOs[i].GetComponent<GridTile>();
            neighbourTiles.Add(nextNTile);

            nWeights[i] = 1 + _gridVisited[nextNTile.GetColumn(), nextNTile.GetRow()];
            nWeights[i] *= GridTile.stateWalkScores[nextNTile.state];

            if (_exitTarget != null)
            nWeights[i] *= _exitHeurstx[nextNTile.GetColumn(), nextNTile.GetRow()];
        }

        float minWeight = float.MaxValue;


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
        if (exitTiles.Count == 0)
        {
            return;
        }

        if (_exitTarget == null)
        {
            _exitTarget = exitTiles[(int)Random.Range(0, exitTiles.Count)];
        }

        _exitCoordinates = _exitTarget.GetComponent<GridTile>().GetCoordinates();

        GenerateExitHeuristics();
    }

    private void GenerateExitHeuristics()
    {

        int gridSize = GridManager.instance.gridSize;

        int eQ = (int) _exitCoordinates.x;
        int eR = (int) _exitCoordinates.y;

        for (int q = 0; q < gridSize; q++) //column
        {
            for (int r = 0; r < gridSize; r++) //row
            {
                float distanceToExit = GridManager.CalculateDistance(q, r, eQ, eR);

                _exitHeurstx[q, r] = distanceToExit;
            }
        }
    }
}