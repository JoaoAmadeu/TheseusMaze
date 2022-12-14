using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer (typeof (RenameAttribute))]
public class RenameEditor : PropertyDrawer
{
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        var renameAttribute = attribute as RenameAttribute;
        EditorGUI.PropertyField (position, property, new GUIContent (renameAttribute.newName));
    }
}