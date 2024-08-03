using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor; 
#endif

public class ObjectiveSystem : MonoBehaviour
{
    public static ObjectiveSystem instance;

    [HideInInspector]
    public List<ObjectiveData> Objectives = new();
    
    void Start()
    {
        instance = this;
        ResetObjectives();
    }

    public void ResetObjectives()
    {
        foreach ( ObjectiveData O in Objectives)
        {
            O.repeatCount = 0;
            O.Complete = false;
        }
        
        GameManager.instance.hudManagerRef.UpdateObjectiveText();
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
                    if ( ++O.repeatCount >= O.buildCount )
                    {
                        O.Complete = true;
                        NPCManager.instance.exitScore += O.npcScoreInc;
                    }
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

                    if ( exitTile == entryTile) continue;

                    if ( GridManager.instance.CheckTileConnection(entryTile, exitTile, pathObjective.target1, null) )
                    {
                        pathObjective.Complete = true;
                        NPCManager.instance.exitScore += pathObjective.npcScoreInc;
                        return;
                    }
                }
            }
        }
    }

    public static string FullStatusText(out bool objectivesDone)
    {
        string statusText = string.Empty;

        int allDone = 0;

        foreach ( ObjectiveData O in instance.Objectives)
        {
            statusText += "[" + (O.Complete ? "X" : "  ") + "] ";
            statusText += O.Description + "\n";
            allDone += O.Complete ? 1 : 0;
        }

        if (allDone == instance.Objectives.Count)
        {
            statusText  = "All Prototype Objectives Complete!\n\n";
            statusText += "Thank you for playing!\n\n Enjoy Building Your Trail!";
        }

        objectivesDone = allDone == instance.Objectives.Count;

        return statusText;
    }
}

[System.Serializable]
public class ObjectiveData
{
    public bool Complete = false;
    public int buildCount  = 1;
    public int repeatCount = 0;
    public int npcScoreInc = 0;
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

#if UNITY_EDITOR 
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
            OS.Objectives[i].objectiveType = (ObjectiveData.Type) EditorGUILayout.EnumPopup(OS.Objectives[i].objectiveType);
            if ( OS.Objectives[i].objectiveType == ObjectiveData.Type.Build ) 
            {
                GUILayout.Label("#", GUILayout.Width(10.0f)); 
                OS.Objectives[i].buildCount = EditorGUILayout.IntField(OS.Objectives[i].buildCount, GUILayout.Width(50.0f));
                OS.Objectives[i].target1 = (GridTile.TileState) EditorGUILayout.EnumPopup(OS.Objectives[i].target1, GUILayout.Width(70.0f));
            }
            else if ( OS.Objectives[i].objectiveType == ObjectiveData.Type.Connect ) 
            {
                GUI.enabled = false;
                OS.Objectives[i].target1 = GridTile.TileState.Dirt;
                OS.Objectives[i].target1 = (GridTile.TileState) EditorGUILayout.EnumPopup(OS.Objectives[i].target1, GUILayout.Width(70.0f));
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Description: ", GUILayout.Width(70.0f)); 
            OS.Objectives[i].Description = GUILayout.TextField(OS.Objectives[i].Description);
            GUILayout.Space(3.0f); if (GUILayout.Button("- Delete", GUILayout.Width(60))) { OS.Objectives.RemoveAt(i--); continue; }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("npcScore+ ", GUILayout.Width(70.0f)); 
            OS.Objectives[i].npcScoreInc = EditorGUILayout.IntField(OS.Objectives[i].npcScoreInc, GUILayout.Width(50.0f));
            GUILayout.EndHorizontal();

            GUILayout.Space(10.0f);
        }
        GUILayout.Space(11.0f);

        GUILayout.BeginHorizontal();
        if ( GUILayout.Button(" + Add Objective ")  ) { OS.Objectives.Add(new ObjectiveData()); }
        GUILayout.EndHorizontal();

        EditorUtility.SetDirty(OS);
    }
}
#endif