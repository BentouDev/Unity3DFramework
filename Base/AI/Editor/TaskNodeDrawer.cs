using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.AI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TaskNode), true)]
public class TaskNodeDrawer : Editor
{
    public TaskNode Node
    {
        get { return (TaskNode)target; }
    }

    public override void OnInspectorGUI()
    {
        InspectorUtils.DrawDefaultScriptField(serializedObject);

        GUILayout.Label("Description", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(Node.Description, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space();
        GUILayout.Label("Parameters", EditorStyles.boldLabel);

        InspectorUtils.DrawDefaultInspectorWithoutScriptField(serializedObject);
    }
}
