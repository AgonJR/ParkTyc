using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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
    [Header("Custom Cursor")]
    public bool useCustomCursor;
    public Texture2D cursorTexture;

    [Header("Debug Panels")]
    public TMP_InputField regenSizeField;
    public TMP_Dropdown loadGridDropdown;


    void Start()
    {
        SelectTileType(GridTile.TileState.Bush);

        ReviewUnlockedButtons();

        DebugPanel_FillGridLoadDropdown();

        if (useCustomCursor) Cursor.SetCursor(cursorTexture, new Vector2(16, 16), CursorMode.Auto);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha0)) SelectTileType(GridTile.TileState.Base );
        if (Input.GetKey(KeyCode.Alpha1)) SelectTileType(GridTile.TileState.Grass);
        if (Input.GetKey(KeyCode.Alpha2)) SelectTileType(GridTile.TileState.Dirt );
        if (Input.GetKey(KeyCode.Alpha3)) SelectTileType(GridTile.TileState.Tree );
        if (Input.GetKey(KeyCode.Alpha4)) SelectTileType(GridTile.TileState.Bush );
        if (Input.GetKey(KeyCode.Alpha5)) SelectTileType(GridTile.TileState.Rock );
        if (Input.GetKey(KeyCode.Alpha6)) SelectTileType(GridTile.TileState.Water);
        if (Input.GetKey(KeyCode.Alpha7)) SelectTileType(GridTile.TileState.Bench);
        if (Input.GetKey(KeyCode.Alpha8)) SelectTileType(GridTile.TileState.Camp);
    }

    public void SelectTileType(GridTile.TileState newType)
    {
        SelectTileType((int)newType);
    }

    public void SelectTileType(int tile)
    {
        int index = (int)tile - 1;

        if (tileButts[index].interactable)
        {
            selectedType = (GridTile.TileState)tile;

            for (int i = 0; i < tileButts.Length; i++)
            {
                var colors = tileButts[i].colors;
                colors.normalColor = i + 1 == (int)selectedType ? selectdColour : defaultColour;
                tileButts[i].colors = colors;
            }
        }
    }

    private void ReviewUnlockedButtons()
    {
        for (int i = 0; i < tileButts.Length; i++)
        {
            tileButts[i].interactable = GridTile.stateUnlockCost[(GridTile.TileState)(i + 1)] <= GameManager.Score;
        }
    }

    public void DisplayScoreChange(int amount)
    {
        string deltaString = (amount >= 0 ? "+" : "-") + amount;
        string scoreString = "Score: " + GameManager.Score.ToString("000");

        ScoreText.text = scoreString;
        DeltaText.text = deltaString;

        ReviewUnlockedButtons();

        ScoreAnimator.Play("UI_ScoreIncreaseAnim");
    }

    public void DisplayVisitorCount()
    {
        string visitorString = "Visitors: " + NPCManager.GetNPCCount();
        VisitorText.text = visitorString;
    }


    // ---
    // DEBUG Panel
    //

    public void DebugPanel_SetUITextGridSize(int newSize)
    {
        regenSizeField.text = newSize + "";
    }

    public void DebugPanel_FillGridLoadDropdown()
    {
        string directoryPath = Path.Combine(Application.dataPath + "/SavedGrids");
        string[] jsonPaths = Directory.GetFiles(directoryPath);

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (string jsonPath in jsonPaths)
        {
            string[] splitPath = jsonPath.Split("/");

            string fileName = splitPath[splitPath.Length - 1];

            if (fileName.EndsWith(".json"))
            {
                fileName = fileName.Split(".")[0];
                options.Add(new TMP_Dropdown.OptionData(fileName));
            }
        }

        loadGridDropdown.ClearOptions();
        loadGridDropdown.AddOptions(options);
    }

    public void DebugPanel_HandleLoadClick()
    {
        string jsonName = loadGridDropdown.options[loadGridDropdown.value].text;
        GridManager.instance.LoadGridFromJSON(jsonName);
    }

    public void DebugPanel_HandleRegenClick()
    {
        int newSize = int.Parse(regenSizeField.text);
        GridManager.instance.ExternalRegenerate(newSize);
    }
}