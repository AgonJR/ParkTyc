using System.Collections.Generic;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    [Range(0, 30)] public float npcSpeed = 5.0f;
    [Space]
    public GameObject nextTarget;
    public float minTrgtDistance;
    public int  maxStepsWhenLost;
    public int   minStepsPreExit;
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
    private GameObject _currTileGO;
    private GameObject _exitTarget;
    private GridTile _exitTile;
    private int _stepCount = 0;
    private int _exitScore = 0;

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

        int gridSizeQ = GridManager.instance.gridSizeQ;
        int gridSizeR = GridManager.instance.gridSizeR;

        _coordinates = new Vector2(spawnQ, spawnR);

        _gridVisited = new int[gridSizeQ, gridSizeR];
        _exitHeurstx = new float[gridSizeQ, gridSizeR];


        for (int q = 0; q < gridSizeQ; q++) //column
        {
            for (int r = 0; r < gridSizeR; r++) //row
            {
                _gridVisited[q, r] = 0;
                _exitHeurstx[q, r] = 1;
            }
        }

        _stepCount = 0;
        _gridVisited[spawnQ, spawnR] = 1;
        _sitCooldown = Mathf.Clamp((int) Random.Range(-3.0f, 6.0f), 0, 9); // So some NPCs won't sit immediately
    }

    public void GridSizeIncreased(int dQ, int dR)
    {
        int gridSizeQ = GridManager.instance.gridSizeQ;
        int gridSizeR = GridManager.instance.gridSizeR;

        int[,] visitCache = new int[gridSizeQ, gridSizeR];
        float[,] exitCache = new float[gridSizeQ, gridSizeR];

        for (int q = 0; q < gridSizeQ; q++) //column
        {
            for (int r = 0; r < gridSizeR; r++) //row
            {
                if ( q < _gridVisited.GetLength(0) && r < _gridVisited.GetLength(1) )
                {
                    visitCache[q, r] = _gridVisited[q, r];
                    exitCache[q, r] = _exitHeurstx[q, r];
                }
                else
                {
                    visitCache[q, r] = 0;
                    exitCache[q, r] = 0;
                }
            }
        }

        _gridVisited = visitCache;
        _exitHeurstx = exitCache;
        _exitTarget  = null;

        if ( _entryCoordinates.x != 0 )
        {
            _entryCoordinates.y += dR;
            _gridVisited[(int) _entryCoordinates.x, (int)_entryCoordinates.y] = 1;
            if (_animatorRef == null ) GridEnterSequence();
        }

        SelectExitTarget(NPCManager.RequestExitTiles(_currTileGO));
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
            SelectExitTarget(NPCManager.RequestExitTiles(_currTileGO));
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

        _currTileGO = nextTarget;
        _coordinates = new Vector2(q, r);

        nextTarget = null;

        _stepCount++;

        // Entered Grid
        if (_entryCoordinates == _coordinates) {  GridEnterSequence(); }

        // Exit Reached
        if (_exitCoordinates == _coordinates) { GridExitSequence(q, r); }

        // Left Grid
        if (_outroStarted && _outroCoordinates == _coordinates)
        {
            GameManager.instance.AddToScore(NPCManager.instance.exitScore + _exitScore);
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

    private void GridEnterSequence()
    {
        if (spwnMesh.enabled)
        {
            spwnMesh.enabled = false;
            entryVFX.SetActive(true);

            modelMale.SetActive(Random.Range(0, 100) > 50);
            modelFeml.SetActive(!modelMale.activeInHierarchy);

            _animatorRef = modelMale.activeInHierarchy ? modelMale.GetComponent<Animator>() : modelFeml.GetComponent<Animator>();
            _animatorRef.SetInteger("animState", 1); // Walk
        }
    }

    private void GridExitSequence(int q, int r)
    {
        int oQ = q;
        int oR = r;

        if (q == 0) { oQ -= 1; }
        else if (r == 0) { oR -= 1; }
        else if (q == GridManager.instance.GetGridSizeQ() - 1) { oQ += 1; }
        else if (r == GridManager.instance.GetGridSizeR() - 1) { oR += 1; }

        _outroStarted = true;
        _outroCoordinates = new Vector2(oQ, oR);
        nextTarget = GridManager.instance.GetBorderTileGO(oQ, oR);

        modelMale.SetActive(false);
        modelFeml.SetActive(false);

        exitMesh.enabled = true;
        exitVFX.SetActive(true);
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
                if (nextNTile.state == GridTile.TileState.Bench && _sitCooldown > 0)
                {
                    continue;
                }

                if (nextNTile.state == GridTile.TileState.Rock)
                {
                    if ( Random.Range(0, 100) > 6 || _sitCooldown > 0 ) { continue; }
                }

                _activitySpotted = true;

                _activityTile = nextNTile;
                _activityTile.Occupy(this);
                nextTarget = _activityTile.gameObject;

                _activityCoordinates = _activityTile.GetCoordinates();

                _gridVisited[nextNTile.GetColumn(), nextNTile.GetRow()]++;

                return;
            }
        }
        _sitCooldown--;

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

            if (_exitTarget != null && _stepCount > minStepsPreExit)
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
            else if ( nWeights[i] == minWeight)
            {
                if ( Random.Range(0, 100) > 50 ) nextTarget = neighbourGOs[i];
            }
        }
        
        _animatorRef.SetInteger("animState", nextTarget == null ? 0 : 1); // Idle : Walk
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
            if ( exitTiles.Count == 1 )
            {
                if ( exitTiles[0].GetComponent<GridTile>().GetCoordinates() == _entryCoordinates ) return;
            }

            do
            {
                _exitTarget = exitTiles[(int)Random.Range(0, exitTiles.Count)];
            } 
            while (_exitTarget.GetComponent<GridTile>().GetCoordinates() == _entryCoordinates );
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
        int gridSizeQ = GridManager.instance.gridSizeQ;
        int gridSizeR = GridManager.instance.gridSizeR;

        int eQ = (int) _exitCoordinates.x;
        int eR = (int) _exitCoordinates.y;

        for (int q = 0; q < gridSizeQ; q++) //column
        {
            for (int r = 0; r < gridSizeR; r++) //row
            {
                float distanceToExit = GridManager.CalculateDistance(q, r, eQ, eR);

                _exitHeurstx[q, r] = distanceToExit;
            }
        }
    }

    private int _activityFrames =  0; // Temporary, keep track of how many frames since started activity interaction
    private int _sitFrames = 300;
    private int _ywnFrames = 100;
    private int _sitCooldown = 0;
    private void ProcessActivity()
    {
        if ( _activityStarted && _activityTile != null )
        {
            _activityFrames++;

            if ( _activityTile.state == GridTile.TileState.Bench )
            {
                if ( _activitySpotted )
                {
                    RotateOnBench();

                    _activitySpotted = false;
                    _animatorRef.Play("Bench_Sit");
                    _animatorRef.SetInteger("animState", 2); // Bench Idle
                    _activityFrames += Random.Range(-30, 51); // slightly randomlize sit duration
                }

                if (_activityFrames > _sitFrames )
                {
                    if ( _animatorRef.GetCurrentAnimatorStateInfo(0).IsName("Bench_Idle") )
                    {
                        _animatorRef.Play("Bench_Stand");
                        _animatorRef.SetInteger("animState", 1); // Walk
                        _sitCooldown = 3;
                    }
                    else if ( _animatorRef.GetCurrentAnimatorStateInfo(0).IsName("Walk01") )
                    {
                        _activityTile.UnOccupy(this);
                        _activityStarted = false;
                        _activityTile = null;
                        _activityFrames = 0;
                        _exitScore += 5;
                    }
                }
            }

            if ( _activityTile.state == GridTile.TileState.Rock )
            {
                if ( _activitySpotted )
                {
                    _activitySpotted = false;
                    _animatorRef.Play("Idle_Yawn");
                    _animatorRef.SetInteger("animState", 0); // Idle
                }

                if (_activityFrames > _ywnFrames )
                {
                    if ( _animatorRef.GetCurrentAnimatorStateInfo(0).IsName("Idle01") )
                    {
                        _animatorRef.SetInteger("animState", 1); // Walk
                        _sitCooldown = 3;
                    }
                    else if ( _animatorRef.GetCurrentAnimatorStateInfo(0).IsName("Walk01") )
                    {
                        _activityTile.UnOccupy(this);
                        _activityStarted = false;
                        _activityTile = null;
                        _activityFrames = 0;
                    }
                }
            }
        }
    }

    public void EndActivity()
    {
        if (_activityStarted)
        {
            _animatorRef.Play("Idle01");
            _animatorRef.SetInteger("animState", 1); // Walk
        }

        _activitySpotted = false;
        _activityStarted = false;
        _activityTile = null;
        _activityFrames = 0;
    }

    public void RotateOnBench()
    {
        Transform mark = _activityTile.GetComponentInChildren<BenchTile>().actMark;
        transform.position = new Vector3(mark.position.x, transform.position.y, mark.position.z);

        int sitRotation = _activityTile.GetComponentInChildren<BenchTile>().getSitRotation();
        transform.localEulerAngles = new Vector3(0, sitRotation, 0);
    }
}