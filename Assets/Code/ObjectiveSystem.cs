using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Xml.Linq;

public class ObjectiveSystem : MonoBehaviour
{
    public static ObjectiveSystem instance;

    public List<ObjectiveData> Objectives = new();
    
    void Start()
    {
        instance = this;
        ResetObjectives();
    }

    public void ResetObjectives()
    {
        Objectives = new();
        SetUpDemoObjectives();
        GameManager.instance.hudManagerRef.UpdateObjectiveText();
    }

    private void SetUpDemoObjectives()
    {
        ObjectiveData O1 = new();
        O1.Description   = "Draw a Path!";
        O1.objectiveType = ObjectiveData.Type.Connect;
        O1.target1       = GridTile.TileState.Dirt;
        O1.target2       = GridTile.TileState.Dirt;

        ObjectiveData O2 = new();
        O2.Description   = "Build a Bench!";
        O2.target1       = GridTile.TileState.Bench;
        O2.objectiveType = ObjectiveData.Type.Build;

        ObjectiveData O3 = new();
        O3.Description   = "Place 3 Rocks!";
        O3.target1       = GridTile.TileState.Rock;
        O3.objectiveType = ObjectiveData.Type.Build;
        O3.buildCount    = 3;

        Objectives.Add(O1);
        Objectives.Add(O2);
        Objectives.Add(O3);
    }

    public static void Ping_TileBuilt(GridTile.TileState tile)
    {
        foreach ( ObjectiveData O in instance.Objectives)
        {
            if ( O.Complete ) continue;

            if (O.objectiveType == ObjectiveData.Type.Build)
            {
                if (O.target1 == tile)
                {
                    O.Complete = ++O.repeatCount >= O.buildCount;
                }
            }

            if (O.objectiveType == ObjectiveData.Type.Connect)
            {
                if (O.target1 == tile) // Currently, only supports 'Dirt Tile' path connections
                {
                    ReviewPathConnection(O);
                }
            }
        }

        GameManager.instance.hudManagerRef.UpdateObjectiveText();
    }

    private static void ReviewPathConnection(ObjectiveData pathObjective)
    {
        NPCManager.ForceScan();

        List<GameObject> ntryTiles = NPCManager.RequestEntryTiles();
        List<GameObject> exitTiles = NPCManager.RequestExitTiles();

        if ( ntryTiles.Count > 0 && exitTiles.Count > 0 )
        {
            foreach( GameObject entry in ntryTiles )
            {
                GridTile entryTile = entry.GetComponent<GridTile>();

                foreach ( GameObject exit in exitTiles )
                {
                    GridTile exitTile = exit.GetComponent<GridTile>();

                    if ( GridManager.instance.CheckTileConnection(entryTile, exitTile, GridTile.TileState.Dirt, null) )
                    {
                        pathObjective.Complete = true;
                        return;
                    }
                }
            }
        }
    }

    public static string FullStatusText()
    {
        string statusText = string.Empty;

        foreach ( ObjectiveData O in instance.Objectives)
        {
            statusText += "[" + (O.Complete ? "X" : "  ") + "] ";
            statusText += O.Description + "\n";
        }

        return statusText;
    }
}

public class ObjectiveData
{
    public bool Complete = false;
    public int buildCount = 1;
    public int repeatCount = 0;
    public string Description = "[notSET]";
    public Type objectiveType = Type.Build;
    public ObjectiveData preRequisite = null;
    public GridTile.TileState target1 = GridTile.TileState.Base;
    public GridTile.TileState target2 = GridTile.TileState.Base;

    public enum Type
    {
        Build,
        Connect
    }
}

[CustomEditor(typeof(ObjectiveSystem))]
public class ObjectiveSystemEditorOverride : Editor
{
    private ObjectiveSystem OS = null;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        OS = (ObjectiveSystem) target;

        GUILayout.Space(10.0f);
        for ( int i = 0; i < OS.Objectives.Count; i++ )
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            GUILayout.Toggle(OS.Objectives[i].Complete, "✓");
            GUI.enabled = true;
            OS.Objectives[i].objectiveType = (ObjectiveData.Type) EditorGUILayout.EnumPopup("Objective Type", OS.Objectives[i].objectiveType);
            if ( OS.Objectives[i].objectiveType == ObjectiveData.Type.Build ) 
            {
                GUILayout.Label("#", GUILayout.Width(10.0f)); 
                OS.Objectives[i].buildCount = EditorGUILayout.IntField(OS.Objectives[i].buildCount, GUILayout.Width(50.0f));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Description: ", GUILayout.Width(70.0f)); 
            OS.Objectives[i].Description = GUILayout.TextField(OS.Objectives[i].Description);
            GUILayout.Space(3.0f); if (GUILayout.Button("- Delete", GUILayout.Width(60))) { OS.Objectives.RemoveAt(i--); continue; }
            GUILayout.EndHorizontal();

            GUILayout.Space(10.0f);
        }
        GUILayout.Space(11.0f);

        GUILayout.BeginHorizontal();
        if ( GUILayout.Button(" + Add Objective ")  ) { OS.Objectives.Add(new ObjectiveData()); }
        GUILayout.EndHorizontal();

        EditorUtility.SetDirty(target);
    }
}
