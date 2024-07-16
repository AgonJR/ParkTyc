using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    static public GridTile.TileState selectedType;

    [Header("Tile Buttons")]
    public Color defaultColour;
    public Color selectdColour;
    [Space]
    public Button[] tileButts;

    [Header("Labels")]
    public TMP_Text ScoreText;
    public TMP_Text DeltaText;
    public Animator ScoreAnimator;
    [Space]
    public TMP_Text VisitorText;



    void Start()
    {
        SelectTileType(GridTile.TileState.Bush);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1)) SelectTileType(GridTile.TileState.Grass);
        if (Input.GetKey(KeyCode.Alpha2)) SelectTileType(GridTile.TileState.Dirt );
        if (Input.GetKey(KeyCode.Alpha3)) SelectTileType(GridTile.TileState.Tree );
        if (Input.GetKey(KeyCode.Alpha4)) SelectTileType(GridTile.TileState.Bush );
        if (Input.GetKey(KeyCode.Alpha5)) SelectTileType(GridTile.TileState.Rock );
        if (Input.GetKey(KeyCode.Alpha6)) SelectTileType(GridTile.TileState.Water);
        if (Input.GetKey(KeyCode.Alpha0)) SelectTileType(GridTile.TileState.Base );
    }

    public void SelectTileType(GridTile.TileState newType)
    {
        SelectTileType((int)newType);
    }

    public void SelectTileType(int tile)
    {
        selectedType = (GridTile.TileState) tile;

        for (int i = 0; i < tileButts.Length; i++)
        {
            var colors = tileButts[i].colors;
            colors.normalColor = i + 1 == (int)selectedType ? selectdColour : defaultColour;
            tileButts[i].colors = colors;
        }
    }

    public void DisplayScoreChange(int amount)
    {
        string sign = amount >= 0 ? "+" : "-";

        string deltaString = sign + amount + "!";
        string scoreString = "Score: " + GameManager.Score.ToString("000");

        ScoreText.text = scoreString;
        DeltaText.text = deltaString;

        ScoreAnimator.Play("UI_ScoreIncreaseAnim");
    }

    public void DisplayVisitorCount()
    {
        string visitorString = "Visitors: " + NPCManager.GetNPCCount();
        VisitorText.text = visitorString;
    }
}