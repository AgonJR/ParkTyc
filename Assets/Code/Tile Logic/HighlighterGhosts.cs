using UnityEngine;

public class HighlighterGhosts : MonoBehaviour
{
    public static HighlighterGhosts instance;

    public bool UseGhosts = true;

    [Header("Ghost Refs")]
    public Material Unlocked;
    public Material Locked;
    [Space]
    public GameObject ghostDirt;
    public GameObject ghostTree;
    public GameObject ghostBush;
    public GameObject ghostRock;
    public GameObject ghostWatr;
    public GameObject ghostBnch;
    public GameObject ghostCamp;
    
    private MeshRenderer[] renderRefs;
    private MeshRenderer[] rockGhostMeshRenderRefs;

    void Awake()
    {
        instance = this;
        GatherRenderRefs();
        ToggleAllGhosts(false);
    }

    private void GatherRenderRefs()
    {
        renderRefs = new MeshRenderer[7];
        renderRefs[0] = ghostDirt.GetComponent<MeshRenderer>();
        renderRefs[1] = ghostTree.GetComponent<MeshRenderer>();
        renderRefs[2] = ghostBush.GetComponent<MeshRenderer>();
        renderRefs[3] = null;
        rockGhostMeshRenderRefs = ghostRock.GetComponentsInChildren<MeshRenderer>();
        renderRefs[4] = ghostWatr.GetComponent<MeshRenderer>();
        renderRefs[5] = ghostBnch.GetComponent<MeshRenderer>();
        renderRefs[6] = ghostCamp.GetComponent<MeshRenderer>();
    }

    public void SetGhost(GridTile.TileState tileState)
    {
        if ( UseGhosts == false )
        {
            return;
        }

        if (renderRefs == null || renderRefs.Length <= 1)
        {
            GatherRenderRefs();
        }

        ToggleAllGhosts(false);

        switch (tileState)
        {
            case GridTile.TileState.Dirt:  ghostDirt.SetActive(true); break;
            case GridTile.TileState.Tree:  ghostTree.SetActive(true); break;
            case GridTile.TileState.Bush:  ghostBush.SetActive(true); break;
            case GridTile.TileState.Rock:  ghostRock.SetActive(true); break;
            case GridTile.TileState.Water: ghostWatr.SetActive(true); break;
            case GridTile.TileState.Bench: ghostBnch.SetActive(true); break;
            case GridTile.TileState.Camp:  ghostCamp.SetActive(true); break;
        }

        UpdateGhostColour(tileState);
    }

    public void UpdateGhostColour(GridTile.TileState tileState)
    {
        if(tileState == GridTile.TileState.Grass ) 
        {

        }
        else if(tileState == GridTile.TileState.Rock ) 
        {
            foreach (MeshRenderer renderRef in rockGhostMeshRenderRefs)
            {
                renderRef.material = GameManager.Score >= GridTile.stateUnlockCost[tileState] ? Unlocked : Locked;
            }
        }
        else
        {
            if ( renderRefs != null && renderRefs.Length > 5 )
            renderRefs[((int) tileState) - 2].material = GameManager.Score >= GridTile.stateUnlockCost[tileState] ? Unlocked : Locked;
        }
    }

    public void HideIfMatch(GridTile.TileState tileState)
    {
        ToggleAllGhosts(false);
        
        if ( tileState != HUDManager.selectedType )
        {
            SetGhost(HUDManager.selectedType);
        }
    }

    private void ToggleAllGhosts(bool enable)
    {
        ghostDirt.SetActive(enable);
        ghostTree.SetActive(enable);
        ghostBush.SetActive(enable);
        ghostRock.SetActive(enable);
        ghostWatr.SetActive(enable);
        ghostBnch.SetActive(enable);
        ghostCamp.SetActive(enable);
    }
}
