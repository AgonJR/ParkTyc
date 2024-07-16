using UnityEngine;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public GameObject npcPrefab;
    [Space]
    [Range(0, 10)] public float spawnWait = 5;
    [Range(0, 30)] public float maxSpawns = 5;
    [Space]

    [Header("Dev Tools")]
    [SerializeField] private List<GameObject> _spwnTiles;
    [SerializeField] private List<GameObject> _exitTiles;
    [SerializeField] private List<GameObject> _spawnedNPCs;
                     private List<NPCBrain> _spndNPCBrains;

    private float spawnDelay = 0;

    public static NPCManager instance;


    void Start()
    {
        instance = this;

        ClearNPCs();
        InvokeRepeating("ScanTiles", 0.0f, 1.0f);
    }

    void ScanTiles()
    {
        _spwnTiles = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.West);

        _exitTiles.Clear();
        _exitTiles.AddRange(GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.East ));
        _exitTiles.AddRange(GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.North));
        _exitTiles.AddRange(GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.South));
    }

    private void Update()
    {
        SpawnCheck();
    }

    public void ClearNPCs()
    {
        if (_spawnedNPCs != null)
        {
            foreach (GameObject npcGO in _spawnedNPCs)
            {
                ClearNPC(npcGO);
            }
        }

        _spawnedNPCs = new List<GameObject>();
        _spndNPCBrains = new List<NPCBrain>();

        spawnDelay = spawnWait;
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

            GameObject spawn = _spwnTiles[(int)Random.Range(0, _spwnTiles.Count)];

            Vector3 spawnPos = new Vector3(spawn.transform.position.x, 2.0f, spawn.transform.position.z);

            GameObject newNPC = GameObject.Instantiate(npcPrefab, spawnPos, npcPrefab.transform.rotation);

            NPCBrain newBrain = newNPC.GetComponent<NPCBrain>();

            GridTile spawnTile = spawn.GetComponent<GridTile>();
            int q = spawnTile.GetColumn();
            int r = spawnTile.GetRow();

            newBrain.Init(q, r);
            newBrain.SelectExitTarget(_exitTiles);

            _spawnedNPCs.Add(newNPC);
            _spndNPCBrains.Add(newBrain);

            spawnDelay = spawnWait;
        }
    }
}