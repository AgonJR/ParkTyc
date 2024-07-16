using UnityEngine;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public GameObject npcPrefab;
    [Space]
    [Range(0, 30)] public float npcSpeed  = 5.0f;
    [Range(0, 10)] public float spawnWait = 5;
    [Range(0, 30)] public float maxSpawns = 5;
    [Space]

    [Header("Dev Tools")]
    [SerializeField] private List<GameObject> spwnTiles;
    [SerializeField] private List<GameObject> exitTiles;
    [SerializeField] private List<GameObject> spawnedNPCs;
                     private List<NPCBrain> spndNPCBrains;

    private float spawnDelay = 0;


    void Start()
    {
        ClearNPCs();
        InvokeRepeating("ScanTiles", 0.0f, 1.0f);
    }

    void ScanTiles()
    {
        spwnTiles = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.West);
        exitTiles = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.East);
    }

    private void Update()
    {
        SpawnCheck();
    }

    public void ClearNPCs()
    {
        if (spawnedNPCs != null)
        {
            foreach (GameObject npcGO in spawnedNPCs)
            {
                Destroy(npcGO);
            }
        }

        spawnedNPCs = new List<GameObject>();
        spndNPCBrains = new List<NPCBrain>();

        spawnDelay = spawnWait;
    }

    private void SpawnCheck()
    {
        if (spawnedNPCs.Count < maxSpawns && npcPrefab != null && spwnTiles.Count > 0)
        {
            if ( spawnDelay > 0 )
            {
                spawnDelay -= Time.deltaTime;
                return;
            }

            GameObject spawn = spwnTiles[(int)Random.Range(0, spwnTiles.Count)];

            Vector3 spawnPos = new Vector3(spawn.transform.position.x, 2.0f, spawn.transform.position.z);

            GameObject newNPC = GameObject.Instantiate(npcPrefab, spawnPos, npcPrefab.transform.rotation);

            NPCBrain newBrain = newNPC.GetComponent<NPCBrain>();

            GridTile spawnTile = spawn.GetComponent<GridTile>();
            int q = spawnTile.GetColumn();
            int r = spawnTile.GetRow();

            newBrain.Init(q, r);
            newBrain.SelectExitTarget(exitTiles);

            spawnedNPCs.Add(newNPC);
            spndNPCBrains.Add(newBrain);

            spawnDelay = spawnWait;
        }
    }
}