////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Reflection;
using UnityEngine;
using UnityEditor;
using static FronkonGames.Artistic.OilPaint.Inspector;

namespace FronkonGames.Artistic.OilPaint
{
  /// <summary> Drawer base. </summary>
  public abstract class Drawer : PropertyDrawer
  {
    protected SerializedProperty properties = null;

    private GUIStyle styleLogo;

    protected abstract void InspectorGUI();

    protected virtual void InspectorChanged() { }

    protected virtual void OnCopy() { }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      properties = property;

      ResetGUI();

      Event evt = Event.current;
      if (evt?.isKey == true && evt.type == EventType.KeyDown && evt.keyCode == KeyCode.C && evt.control == true)
      {
        OnCopy();

        evt.Use();
      }

      if (styleLogo == null)
      {
        Font font = null;
        string[] ids = AssetDatabase.FindAssets("FronkonGames-Black");
        for (int i = 0; i < ids.Length; ++i)
        {
          string fontPath = AssetDatabase.GUIDToAssetPath(ids[i]);
          if (fontPath.Contains(".otf") == true)
          {
            font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            break;
          }
        }

        if (font != null)
        {
          styleLogo = new GUIStyle(EditorStyles.boldLabel)
          {
            font = font,
            alignment = TextAnchor.LowerLeft,
            fontSize = 24
          };
        }
      }

      EditorGUI.BeginChangeCheck();

      BeginVertical();
      {
        /////////////////////////////////////////////////
        // Description.
        /////////////////////////////////////////////////
        if (styleLogo != null)
        {
          EditorGUILayout.BeginHorizontal();
          {
            FlexibleSpace();

            GUILayout.Label(Constants.Asset.Name, styleLogo);
          }
          EditorGUILayout.EndHorizontal();

          EditorGUILayout.BeginHorizontal();
          {
            FlexibleSpace();

            GUILayout.Label(Constants.Asset.Description, EditorStyles.miniLabel);
          }
          EditorGUILayout.EndHorizontal();

          Separator();
        }

        InspectorGUI();
      }
      EndVertical();

      if (EditorGUI.EndChangeCheck() == true)
        InspectorChanged();
    }

    protected T GetSettings<T>()
    {
      object target = GetValue(properties.serializedObject.targetObject, "settings");

      return (T)target;
    }

    protected string ToString(float value) { return $"{value}f".Replace(",", "."); }

    protected string ToString(Color value) { return $"new Color({ToString(value.r)}, {ToString(value.g)}, {ToString(value.b)})"; }
    //=> $"new Color{value.ToString().Replace("RGBA", string.Empty)}";

    private static object GetValue(object source, string name)
    {
      if (source == null)
        return null;

      var type = source.GetType();
      var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
      if (f == null)
      {
        var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return p?.GetValue(source, null);
      }

      return f.GetValue(source);
    }
  }
}
