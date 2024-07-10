using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public GameObject NPCPrefab;

    [Header("Dev Tools")]
    [SerializeField] private GameObject spwnTile;
    [SerializeField] private GameObject exitTile;

    private GameObject spawnedNPC;


    void Start()
    {
        InvokeRepeating("ScanTiles", 0.0f, 1.0f);
    }

    void ScanTiles()
    {
        spwnTile = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.West);
        exitTile = GridManager.instance.ScanEdgeTiles(GridTile.TileState.Dirt, GridManager.Direction.East);
    }

    private void Update()
    {
        if (spawnedNPC == null && NPCPrefab != null && spwnTile != null)
        {
            Vector3 spawnPos = new Vector3(spwnTile.transform.position.x, 2.0f, spwnTile.transform.position.z);

            spawnedNPC = GameObject.Instantiate(NPCPrefab, spawnPos, NPCPrefab.transform.rotation);

            spawnedNPC.name = "N P C";
        }
    }

}
