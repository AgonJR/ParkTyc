using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;

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
    public int exitScore = 5;

    [Header("Dev Tools")]
    [SerializeField] private List<GameObject> _spwnTiles;
    [SerializeField] private List<GameObject> _exitTiles;
    [SerializeField] private List<GameObject> _spawnedNPCs;
                     private List<NPCBrain> _spndNPCBrains;
                     private List<KeyValuePair<GameObject, List<GameObject>>> _connectedPathPairs; // <entry, exit> pairs that have a full connection

    private float spawnDelay = 0;
    private int maxSpawnTileChecks = 30;
    private int spawnCheckCount = 0;


    void Start()
    {
        instance = this;
        ClearNPCs();
    }

    void ScanTiles()
    {
        _spwnTiles = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.West);
        _spwnTiles.AddRange(GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.South));

        _exitTiles = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.Any);

        if (_spwnTiles.Count == 0)
        {
            if (++spawnCheckCount >= maxSpawnTileChecks)
            {
                var westernGrassTiles = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Grass, GridManager.Direction.West);
                if (westernGrassTiles.Count > 0) { _spwnTiles.Add(westernGrassTiles[Random.Range(0, westernGrassTiles.Count)]); }
            }
        }
        else
        {
            spawnCheckCount = 0;
        }

        if ( _spwnTiles.Count > 0 && _exitTiles.Count > 0 )
        {
            _connectedPathPairs = new List<KeyValuePair<GameObject, List<GameObject>>>();

            for ( int i = 0; i < _spwnTiles.Count; i++ )
            {
                for ( int j = 0; j < _exitTiles.Count; j++ )
                {
                    if ( _spwnTiles[i] == _exitTiles[j] ) continue;

                    if ( GridManager.instance.CheckTileConnection(_spwnTiles[i], _exitTiles[j], GridTile.TileState.Dirt, null) )
                    {
                        _connectedPathPairs.Add(new(_spwnTiles[i], new List<GameObject>(){_exitTiles[j]}));
                    }
                }
            }
        }
    }

    public static void ForceScan()
    {
        instance.spawnCheckCount--;
        instance.ScanTiles();
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

    public static List<GameObject> RequestExitTiles(GameObject entryTileGO = null)
    {
        if ( entryTileGO != null && instance._connectedPathPairs != null && instance._connectedPathPairs.Count > 0)
        {
            foreach ( var kvp in instance._connectedPathPairs )
            {
                if (kvp.Key == entryTileGO)
                {
                    return kvp.Value;
                }
            }
        }

        return instance._exitTiles;
    }

    public static List<GameObject> RequestEntryTiles()
    {
        return instance._spwnTiles;
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

            // Check if spawning West or South
            int spawnQ = spawnTile.GetColumn() == 0 ? spawnTile.GetColumn() - 1 : spawnTile.GetColumn();
            int spawnR = spawnTile.GetRow() == GridManager.instance.GetGridSizeR() - 1 ? spawnTile.GetRow() + 1 : spawnTile.GetRow();

            Vector3 spawnTilePos = GridManager.instance.CalculateTilePosition(spawnQ, spawnR);
            Vector3 spawnPos = new Vector3(spawnTilePos.x, 2.0f, spawnTilePos.z);

            // Spawn Objects
            GameObject newNPC  = GameObject.Instantiate(npcPrefab, spawnPos, npcPrefab.transform.rotation);
            NPCBrain newBrain  = newNPC.GetComponent<NPCBrain>();

            // Initialize Brain
            newBrain.Init(spawnTile.GetColumn(), spawnTile.GetRow());
            newBrain.SelectExitTarget(RequestExitTiles(entryTileGO));
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

    public void PingNPCsGridSizeIncreased(int dQ, int dR)
    {
        foreach ( NPCBrain npc in _spndNPCBrains )
        {
            npc.GridSizeIncreased(dQ, dR);
        }
    }
}