using System.Collections;
using System.Collections.Generic;
using Framework;
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

    public static float GetDefaultInspectorHeight(SerializedObject obj,
        System.Predicate<SerializedProperty> predicate = null)
    {
        SerializedProperty iterator = obj.GetIterator();

        float height = 0;
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            if (!enterChildren && (predicate == null || predicate(iterator)))
            {
                height += EditorGUI.GetPropertyHeight(iterator);
            }

            enterChildren = false;
        }

        return height;
    }

    public static bool DrawDefaultInspector(Rect rect, SerializedObject obj, System.Predicate<SerializedProperty> predicate = null)
    {
        EditorGUI.BeginChangeCheck();
        obj.Update();
        SerializedProperty iterator = obj.GetIterator();

        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            if (!enterChildren && (predicate == null || predicate(iterator)))
            {
                EditorGUI.PropertyField(rect, iterator, true);
                rect.y += EditorGUI.GetPropertyHeight(iterator);
            }

            enterChildren = false;
        }

        obj.ApplyModifiedProperties();
        return EditorGUI.EndChangeCheck();
    }

    public static void DrawFoldableProperty(SerializedProperty prop, string displayName = "")
    {
        if (string.IsNullOrEmpty(displayName))
            displayName = prop.displayName;

        prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, new GUIContent(displayName));
        if (prop.isExpanded)
        {
            EditorGUILayout.PropertyField(prop);
        }
    }
}
