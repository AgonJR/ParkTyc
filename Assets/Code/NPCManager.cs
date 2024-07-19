using UnityEngine;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public static NPCManager instance;

    [Header("NPC Data")]
    public GameObject npcPrefab;
    [Space]
    [Range(0, 10)] public float spawnWait = 5;
    [Range(0,  3)] public float wVariance = 1;
    [Space]
    [Range(0, 30)] public float maxSpawns = 5;
    [Space]

    [Header("Dev Tools")]
    [SerializeField] private List<GameObject> _spwnTiles;
    [SerializeField] private List<GameObject> _exitTiles;
    [SerializeField] private List<GameObject> _spawnedNPCs;
                     private List<NPCBrain> _spndNPCBrains;

    private float spawnDelay = 0;
    private int maxSpawnTileChecks = 10;
    private int spawnCheckCount = 0;


    void Start()
    {
        instance = this;

        ClearNPCs();
        InvokeRepeating("ScanTiles", 0.0f, 1.0f);
    }

    void ScanTiles()
    {
        _spwnTiles = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.West);

        _exitTiles = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.East);

        if (_exitTiles.Count == 0)
        {
            _exitTiles.AddRange(GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.North));
            _exitTiles.AddRange(GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.South));
        }

        if (_spwnTiles.Count == 0)
        {
            if (++spawnCheckCount >= maxSpawnTileChecks)
            {
                var westernGrassTiles = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Grass, GridManager.Direction.West);
                _spwnTiles.Add(westernGrassTiles[Random.Range(0, westernGrassTiles.Count)]);
            }
        }
        else
        {
            spawnCheckCount = 0;
        }
    }

    private void Update()
    {
        SpawnCheck();
    }

    public void ClearNPCs()
    {
          if (_spawnedNPCs != null && _spawnedNPCs.Count > 0)
        { do { ClearNPC(_spawnedNPCs[0]); } while (_spawnedNPCs.Count > 0); }

        _spawnedNPCs = new List<GameObject>();
        _spndNPCBrains = new List<NPCBrain>();

        spawnDelay = spawnWait;
        spawnCheckCount = 0;
    }

    public void ClearNPC(GameObject NPC)
    {
        for (int i = 0; i < _spawnedNPCs.Count; i++)
        {
            if (_spawnedNPCs[i] == NPC)
            {
                _spawnedNPCs.RemoveAt(i);
                _spndNPCBrains.RemoveAt(i);

                i--;

                Destroy(NPC);

                GameManager.instance.hudManagerRef.DisplayVisitorCount();
            }
        }
    }

    public static List<GameObject> RequestExitTiles()
    {
        return instance._exitTiles;
    }

    private void SpawnCheck()
    {
        if (_spawnedNPCs.Count < maxSpawns && npcPrefab != null && _spwnTiles.Count > 0)
        {
            if ( spawnDelay > 0 )
            {
                spawnDelay -= Time.deltaTime;
                return;
            }

            // Select Spawn Position
            GameObject entryTileGO = _spwnTiles[(int)Random.Range(0, _spwnTiles.Count)];
            GridTile spawnTile = entryTileGO.GetComponent<GridTile>();

            // Assuming Left Column Only for Spawn !
            Vector3 spawnTilePos = GridManager.instance.CalculateTilePosition(spawnTile.GetColumn() - 1, spawnTile.GetRow());
            Vector3 spawnPos = new Vector3(spawnTilePos.x, 2.0f, spawnTilePos.z);

            // Spawn Objects
            GameObject newNPC  = GameObject.Instantiate(npcPrefab, spawnPos, npcPrefab.transform.rotation);
            NPCBrain newBrain  = newNPC.GetComponent<NPCBrain>();

            // Initialize Brain
            newBrain.Init(spawnTile.GetColumn(), spawnTile.GetRow());
            newBrain.SelectExitTarget(_exitTiles);
            newBrain.nextTarget = entryTileGO;

            // Store References
            _spawnedNPCs.Add(newNPC);
            _spndNPCBrains.Add(newBrain);

            spawnDelay = spawnWait + Random.Range(wVariance * -1, wVariance);

            // Update U.I.
            GameManager.instance.hudManagerRef.DisplayVisitorCount();
        }
    }

    public static int GetNPCCount()
    {
        return instance._spawnedNPCs.Count;
    }
}