using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (LevelScriptable))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI ()
    {
        DrawDefaultInspector();
        EditorGUILayout.Separator ();
        EditorGUILayout.Separator ();
        EditorGUILayout.Separator ();

        // Create a button in bold style, that opens a window for editing the LevelScriptable
        GUIContent content = new GUIContent ("Open Level Editor");
        GUIStyle style = new GUIStyle (GUI.skin.button);
        float minWidth, maxWidth;

        style.stretchWidth = false;
        style.fontStyle = FontStyle.Bold;
        style.CalcMinMaxWidth (content, out minWidth, out maxWidth);

        var rect = EditorGUILayout.GetControlRect ();
        rect = new Rect (((rect.width * 0.5f) - minWidth * 0.5f), rect.y, minWidth * 1.5f, rect.height * 1.8f);

        if (GUI.Button (rect, content, style)) {
            ArenaWindow.StartEditor (target as LevelScriptable);
        }
    }
}