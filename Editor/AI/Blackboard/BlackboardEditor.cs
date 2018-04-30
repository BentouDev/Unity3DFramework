using System.Collections;
using System.Collections.Generic;
using Framework.AI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Blackboard))]
public class BlackboardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Go through all values in blackboard and 
    }
}
