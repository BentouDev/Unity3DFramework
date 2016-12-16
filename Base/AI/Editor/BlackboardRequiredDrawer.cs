using System.Collections;
using System.Collections.Generic;
using Framework.AI;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Blackboard.Required))]
public class BlackboardRequiredDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var editor = BehaviourTreeEditor.GetInstance();
        if (!editor)
            return;

        // Check if nodes tree is the same as editor asset
        // If so, allow editing
        // On click on field show popup with available blackboard parameters, check type in there
        // Save key name 

        var asTaskNode = (TaskNode)property.serializedObject.targetObject;

        if (!editor.CanEditNode(asTaskNode))
            return;

        var parameters = editor.GetCurrentAsset().Parameters;

        label = EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
        
        EditorGUI.indentLevel = 0;
        // EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("position"), GUIContent.none);

        EditorGUI.BeginChangeCheck();


        EditorGUI.Popup(position, GUIContent.none, -1, new[] {new GUIContent("Sth"), new GUIContent("Other"), },(GUIStyle) "ObjectField");

        //if (GUI.Button(position, asTaskNode ? asTaskNode.Name : "null", (GUIStyle) "ObjectField"))
        {
            
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            
        }

        EditorGUI.EndProperty();

        /*EditorGUI.BeginChangeCheck();
        var newScene = EditorGUI.ObjectField(position, oldPath, typeof(SceneAsset), false) as SceneAsset;

        if (EditorGUI.EndChangeCheck())
        {
            var newPath = AssetDatabase.GetAssetPath(newScene);
            property.stringValue = newPath;
        }*/
    }
}
