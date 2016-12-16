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
        GUILayout.TextArea("Description", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(Node.Description);

        EditorGUILayout.Space();
        GUILayout.Label("Parameters", EditorStyles.boldLabel);

        DrawDefaultInspector();
    }
}
