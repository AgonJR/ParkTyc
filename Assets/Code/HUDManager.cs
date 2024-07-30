using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class HUDManager : MonoBehaviour
{
    static public GridTile.TileState selectedType;

    [Header("Panels")]
    //public GameObject BuildBlocker;
    public GameObject BuildMenu;
    public GameObject NatureMenu;
    [Header("Tile Buttons")]
    public Color defaultColour;
    public Color selectdColour;
    [Space]
    public Button[] tileButts;

    [Header("Labels")]
    public TMP_Text ScoreText;
    public TMP_Text DeltaText;
    public TMP_Text NegDtText;
    public Animator deltaAnimator;
    public Animator negDtAnimator;
    [Space]
    public TMP_Text VisitorText;
    [Header("Custom Cursor")]
    public bool useCustomCursor;
    public Texture2D cursorTexture;

    [Header("Debug Panels")]
    public TMP_InputField regenSizeQField;
    public TMP_InputField regenSizeRField;
    public TMP_Dropdown loadGridDropdown;

    private BuildMode currentMode = BuildMode.Nature;

    private enum BuildMode
    {
        None, //null?
        Nature,
        //Menu,
        Amenities
    }


    void Start()
    {
        SelectTileType(GridTile.TileState.Bush);
        BuildMenu.SetActive(false);

        ReviewUnlockedButtons();

        DebugPanel_FillGridLoadDropdown();

        if (useCustomCursor) Cursor.SetCursor(cursorTexture, new Vector2(16, 16), CursorMode.Auto);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1)) SelectTileType(GridTile.TileState.Grass);
        if (Input.GetKey(KeyCode.Alpha2)) SelectTileType(GridTile.TileState.Dirt);
        if (Input.GetKey(KeyCode.Alpha3)) SelectTileType(GridTile.TileState.Tree);
        if (Input.GetKey(KeyCode.Alpha4)) SelectTileType(GridTile.TileState.Bush);
        if (Input.GetKey(KeyCode.Alpha5)) SelectTileType(GridTile.TileState.Rock);
        if (Input.GetKey(KeyCode.Alpha6)) SelectTileType(GridTile.TileState.Water);
        if (Input.GetKey(KeyCode.Alpha7)) SelectTileType(GridTile.TileState.Bench);
        if (Input.GetKey(KeyCode.Alpha8)) SelectTileType(GridTile.TileState.Camp);

        if (currentMode == BuildMode.Amenities)
        {
            BuildMenu.SetActive(true);
            //BuildBlocker.SetActive(true);
        }
        else
        {
            BuildMenu.SetActive(false);
            //BuildBlocker.SetActive(false);
        }

        if (currentMode == BuildMode.Nature)
        {
            NatureMenu.SetActive(true);
        }
        else
        {
            NatureMenu.SetActive(false);
        }
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

    public void SelectBuildMode(int mode)
    {
        currentMode = (BuildMode)mode;
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
        string deltaString = (amount > 0 ? "+" : "-") + amount;
        string scoreString = "Kash £ " + GameManager.Score;

        ScoreText.text = scoreString;
        DeltaText.text = amount > 0 ? deltaString : string.Empty;
        NegDtText.text = amount < 0 ? deltaString : string.Empty;

        ReviewUnlockedButtons();

        deltaAnimator.Play("UI_ScoreIncreaseAnim");
        negDtAnimator.Play("UI_ScoreIncreaseAnim");
    }

    public void DisplayVisitorCount()
    {
        string visitorString = "Visitors: " + NPCManager.GetNPCCount();
        VisitorText.text = visitorString;
    }


    // ---
    // DEBUG Panel
    //

    public void DebugPanel_SetUITextGridSize(int newSizeQ, int newSizeR)
    {
        regenSizeQField.text = newSizeQ + "";
        regenSizeRField.text = newSizeR + "";
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
        int newQ = int.Parse(regenSizeQField.text);
        int newR = int.Parse(regenSizeRField.text);
        GridManager.instance.ExternalRegenerate(newQ, newR);
    }
}