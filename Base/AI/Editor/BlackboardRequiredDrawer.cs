using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using Framework.AI;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Blackboard.Required))]
public class BlackboardRequiredDrawer : PropertyDrawer
{
    /*public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
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
        
        List<GUIContent> guiContent = new List<GUIContent>();
        foreach (GenericParameter parameter in parameters)
        {
            guiContent.Add(new GUIContent(string.Format("{0} ({1})", parameter.Name, parameter.HoldType)));
        }

        int index = -1;
        GenericParameter value;
        if (asTaskNode.BlackboardRequired.TryGetValue(property.displayName, out value))
        {
            index = parameters.FindIndex(p => p.Name == value.Name && p.HoldType == value.HoldType);
        }

        int result = EditorGUI.Popup(position, GUIContent.none, index, guiContent.ToArray(), EditorStyles.objectField);

        if (result >= 0 && result < parameters.Count)
        {
            var parameter = parameters[result];
            asTaskNode.BlackboardRequired[property.displayName] = parameter;
        }

        //if (GUI.Button(position, asTaskNode ? asTaskNode.Name : "null", (GUIStyle) "ObjectField"))
        {
            
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            
        }

        EditorGUI.EndProperty();

        /EditorGUI.BeginChangeCheck();
        var newScene = EditorGUI.ObjectField(position, oldPath, typeof(SceneAsset), false) as SceneAsset;

        if (EditorGUI.EndChangeCheck())
        {
            var newPath = AssetDatabase.GetAssetPath(newScene);
            property.stringValue = newPath;
        }/
    }*/
}
