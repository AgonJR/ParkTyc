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
        MoveNPCs();
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

            spawnedNPC.name = "N P C";

            spawnDelay = spawnWait;
        }
    }

    private void MoveNPCs()
    {

        if (spawnedNPC != null)
        {
            if (exitTiles.Count > 0)
            {
                GameObject exit = spndNPCBrain.GetTargetExitTile(exitTiles);

                Vector3 direction = exit.transform.position - spawnedNPC.transform.position;

                direction.y = 0.0f; 

                spawnedNPC.transform.position += direction.normalized * npcSpeed * Time.deltaTime;
            }
        }
    }


}
