using System.Collections.Generic;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    [Range(0, 30)] public float npcSpeed = 5.0f;
    [Space]
    public GameObject nextTarget;
    public float minTrgtDistance;
    public int  maxStepsWhenLost;
    [Header("Transition Data")]
    public MeshRenderer spwnMesh;
    public MeshRenderer exitMesh;
    [Space]
    public GameObject  modelMale;
    public GameObject  modelFeml;
    [Space]
    public GameObject   entryVFX;
    public GameObject    exitVFX;
    [Space]

    private Animator _animatorRef;

    private int[,] _gridVisited;
    private float[,] _exitHeurstx;
    private GameObject _exitTarget;
    private GridTile _exitTile;
    private int _stepCount = 0;

    private Vector2 _coordinates;
    private Vector2 _exitCoordinates;
    private Vector2 _entryCoordinates;
    private Vector2 _outroCoordinates;
    private bool _outroStarted = false;

    private bool _activitySpotted = false;
    private bool _activityStarted = false;
    private GridTile _activityTile = null;
    private Vector2 _activityCoordinates;


    public void Init(int spawnQ, int spawnR)
    {
        gameObject.name = "N P C  (" + spawnQ + " , " + spawnR + ")";

        _entryCoordinates = new Vector2(spawnQ, spawnR);

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

        _stepCount = 0;
        _gridVisited[spawnQ, spawnR] = 1;
    }

    private void FixedUpdate()
    {
        ProcessActivity();
        ProcessMovement();
    }

    private void ProcessMovement()
    {
        if (_exitTarget == null || GridTile.stateWalkScores[_exitTile.state] < 0)
        {
            SelectExitTarget(NPCManager.RequestExitTiles());
        }

        if (nextTarget == null && _activitySpotted == false && _activityStarted == false)
        {
            SelectNextTargetTile();
        }

        if (nextTarget != null)
        {
            // Move Towards Target
            Vector3 direction = (nextTarget.transform.position - transform.position).normalized;

            direction.y = 0.0f;

            transform.position += npcSpeed * Time.deltaTime * direction;

            // Rotate to Face Direction
            if (direction != Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * npcSpeed);
            }

            // Check If Target Reached
            float distance = Vector3.Distance(transform.position, nextTarget.transform.position);

            if (distance < minTrgtDistance) { ProcessTargetReached(); }
        }
    }

    private void ProcessTargetReached()
    {
        GridTile targetTile = _outroStarted ? null : nextTarget.GetComponent<GridTile>();

        int q = _outroStarted ? (int)_outroCoordinates.x : targetTile.GetColumn();
        int r = _outroStarted ? (int)_outroCoordinates.y : targetTile.GetRow();

        gameObject.name = "N P C  (" + q + " , " + r + ")";

        _coordinates = new Vector2(q, r);

        nextTarget = null;

        _stepCount++;

        // Entered Grid
        if (_entryCoordinates == _coordinates)
        {
            spwnMesh.enabled = false;
            entryVFX.SetActive(true);

            modelMale.SetActive(Random.Range(0, 100) > 50);
            modelFeml.SetActive(!modelMale.activeInHierarchy);

            _animatorRef = modelMale.activeInHierarchy ? modelMale.GetComponent<Animator>() : modelFeml.GetComponent<Animator>();
            _animatorRef.SetInteger("animState", 1); // Walk
        }

        // Exit Reached
        if (_exitCoordinates == _coordinates)
        {
            int oQ = q;
            int oR = r;

            if (q == 0) { oQ -= 1; }
            else if (r == 0) { oR -= 1; }
            else if (q == GridManager.instance.GetGridSize() - 1) { oQ += 1; }
            else if (r == GridManager.instance.GetGridSize() - 1) { oR += 1; }

            _outroStarted = true;
            _outroCoordinates = new Vector2(oQ, oR);
            nextTarget = GridManager.instance.GetBorderTileGO(oQ, oR);

            modelMale.SetActive(false);
            modelFeml.SetActive(false);

            exitMesh.enabled = true;
            exitVFX.SetActive(true);
        }

        // Left Grid
        if (_outroStarted && _outroCoordinates == _coordinates)
        {
            GameManager.instance.AddToScore(5);
            NPCManager.instance.ClearNPC(gameObject);
        }

        if (_outroStarted == false) _gridVisited[q, r]++;

        // Activity Reached
        if (_activitySpotted && _activityCoordinates == _coordinates)
        {
            _activityStarted = true;
        }

        return;
    }

    private void SelectNextTargetTile()
    {
        List<GameObject> neighbourGOs = GridManager.instance.GetNeighbouringTiles((int)_coordinates.x, (int)_coordinates.y);
        List<GridTile> neighbourTiles = new List<GridTile>();

        //
        // Check For Possible Interactions
        //

        for ( int i = 0; i < neighbourGOs.Count; i++ )
        {
            GridTile nextNTile = neighbourGOs[i].GetComponent<GridTile>();
            
            if ( nextNTile.isActivity && nextNTile.curOccupancy < nextNTile.maxCapacity && _gridVisited[nextNTile.GetColumn(), nextNTile.GetRow()] == 0)
            {
                _activitySpotted = true;

                _activityTile = nextNTile;
                _activityTile.curOccupancy++;
                nextTarget = _activityTile.gameObject;

                _activityCoordinates = _activityTile.GetCoordinates();
                _gridVisited[nextNTile.GetColumn(), nextNTile.GetRow()]++;

                return;
            }
        }

        //
        // Move Towards Exit
        //

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
            if (nWeights[i] < 0) { continue; }

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
            if (_stepCount >= maxStepsWhenLost)
            {
                SelectExitTarget(GridManager.instance.ScanEdgeTiles(GridTile.TileState.Grass, GridManager.Direction.Any));
            }

            return;
        }

        if (_exitTarget == null)
        {
            _exitTarget = exitTiles[(int)Random.Range(0, exitTiles.Count)];
        }

        if (_exitTarget != null)
        {
            _exitTile = _exitTarget.GetComponent<GridTile>();
            _exitCoordinates = _exitTile.GetCoordinates();

            GenerateExitHeuristics();
        }
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

    private int _activityFrames =  0; // Temporary, keep track of how many frames since started activity interaction
    private int _sitFrames = 300;
    private void ProcessActivity()
    {
        if ( _activityStarted && _activityTile != null )
        {
            _activityFrames++;

            if ( _activityTile.state == GridTile.TileState.Bench )
            {
                if ( _activitySpotted )
                {
                    Transform mark = _activityTile.GetComponentInChildren<BenchTile>().actMark;
                    transform.position = new Vector3(mark.position.x, transform.position.y, mark.position.z);

                    int sitRotation = _activityTile.GetComponentInChildren<BenchTile>().getSitRotation();
                    transform.localEulerAngles = new Vector3(0, sitRotation, 0);

                    _activitySpotted = false;
                    _animatorRef.Play("Bench_Sit");
                    _animatorRef.SetInteger("animState", 2); // Bench Idle
                }

                if (_activityFrames > _sitFrames )
                {
                    if ( _animatorRef.GetCurrentAnimatorStateInfo(0).IsName("Bench_Idle") )
                    {
                        _animatorRef.Play("Bench_Stand");
                        _animatorRef.SetInteger("animState", 1); // Walk
                    }
                    else if ( _animatorRef.GetCurrentAnimatorStateInfo(0).IsName("Walk01") )
                    {
                        _activityTile.curOccupancy--;
                        _activityStarted = false;
                        _activityTile = null;
                        _activityFrames = 0;
                    }
                }
            }
        }
    }
}