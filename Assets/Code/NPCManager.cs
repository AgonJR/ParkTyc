using UnityEngine;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public GameObject npcPrefab;
    [Space]
    [Range(0, 30)] public float npcSpeed  = 5.0f;
    [Range(0, 10)] public float spawnWait = 5;
    [Space]

    [Header("Dev Tools")]
    [SerializeField] private List<GameObject> spwnTiles;
    [SerializeField] private List<GameObject> exitTiles;

    private GameObject spawnedNPC;
    private NPCBrain spndNPCBrain;
    private float spawnDelay = 0;


    void Start()
    {
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

    private void SpawnCheck()
    {
        if (spawnedNPC == null && npcPrefab != null && spwnTiles.Count > 0)
        {
            if ( spawnDelay > 0 )
            {
                spawnDelay -= Time.deltaTime;
                return;
            }

            GameObject spawn = spwnTiles[(int)Random.Range(0, spwnTiles.Count)];

            Vector3 spawnPos = new Vector3(spawn.transform.position.x, 2.0f, spawn.transform.position.z);

            spawnedNPC = GameObject.Instantiate(npcPrefab, spawnPos, npcPrefab.transform.rotation);

            spndNPCBrain = spawnedNPC.GetComponent<NPCBrain>();

            GridTile spawnTile = spawn.GetComponent<GridTile>();
            int q = spawnTile.GetColumn();
            int r = spawnTile.GetRow();

            spndNPCBrain.Init(q, r);
            spndNPCBrain.SelectExitTarget(exitTiles);

            spawnDelay = spawnWait;
        }
    }
}