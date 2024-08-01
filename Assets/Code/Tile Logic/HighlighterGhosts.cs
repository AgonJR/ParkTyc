using UnityEngine;

public class HighlighterGhosts : MonoBehaviour
{
    public static HighlighterGhosts instance;

    public bool UseGhosts = true;

    [Header("Ghost Refs")]
    public GameObject ghostDirt;
    public GameObject ghostTree;
    public GameObject ghostBush;
    public GameObject ghostRock;
    public GameObject ghostWatr;
    public GameObject ghostBnch;
    public GameObject ghostCamp;

    void Awake()
    {
        instance = this;
        ToggleAllGhosts(false);
    }

    public void SetGhost(GridTile.TileState tileState)
    {
        if ( UseGhosts == false )
        {
            return;
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
