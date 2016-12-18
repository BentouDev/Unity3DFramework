using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class InspectorUtils
{
    public static bool DrawDefaultInspectorWithoutScriptField(SerializedObject serializedObject)
    {
        return DrawDefaultInspector(serializedObject, (p) =>
        {
            var fieldInfo = serializedObject.targetObject.GetType().GetField(p.name);
            if (fieldInfo != null)
            {
                return fieldInfo.FieldType != typeof(MonoScript);
            }

            return false;
        });
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
