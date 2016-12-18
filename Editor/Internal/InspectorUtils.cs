using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class InspectorUtils
{
    public static void DrawDefaultScriptField(SerializedObject obj)
    {
        obj.Update();
        SerializedProperty iterator = obj.GetIterator();

        GUI.enabled = false;

        bool firstChild = true;
        while (iterator.NextVisible(true) && firstChild)
        {
            EditorGUILayout.PropertyField(iterator, true);
            firstChild = false;
        }

        GUI.enabled = true;
    }

    public static bool DrawDefaultInspectorWithoutScriptField(SerializedObject obj)
    {
        EditorGUI.BeginChangeCheck();
        obj.Update();
        SerializedProperty iterator = obj.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            if (!enterChildren)
            {
                EditorGUILayout.PropertyField(iterator, true);
            }

            enterChildren = false;
        }
        obj.ApplyModifiedProperties();
        return EditorGUI.EndChangeCheck();
    }

    public static bool DrawDefaultInspector(SerializedObject obj, System.Predicate<SerializedProperty> predicate = null)
    {
        EditorGUI.BeginChangeCheck();
        obj.Update();
        SerializedProperty iterator = obj.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            if (predicate == null || predicate(iterator))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }

            enterChildren = false;
        }
        obj.ApplyModifiedProperties();
        return EditorGUI.EndChangeCheck();
    }
}
