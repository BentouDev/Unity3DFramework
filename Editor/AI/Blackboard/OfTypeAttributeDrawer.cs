using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

//[CustomPropertyDrawer(typeof(Blackboard.OfTypeAttribute))]
public class OfTypeAttributeDrawer : PropertyDrawer
{
    /*public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var editor = BehaviourTreeEditor.GetInstance();
        if (!editor)
            return;

        // Check if nodes tree is the same as editor asset
        // If so, allow editing
        // On click on field show popup with available blackboard parameters, check type in there
        // Save key name to Parameter dictionary

        var asTaskNode = (TaskNode)property.serializedObject.targetObject;
        
        // Defensive sanity check, may be redundant
        if (!editor.CanEditNode(asTaskNode))
            return;

        var ofType = attribute as Blackboard.OfTypeAttribute;
        if (ofType == null || ofType.RequiredType == null)
        {
            EditorGUI.HelpBox(position, "Unable to get OfType attribute or its RequiredType!", MessageType.Error);
            return;
        }

        var parameters = editor.GetCurrentAsset().Parameters;

        label = EditorGUI.BeginProperty(position, label, property);
        {
            position = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));

            EditorGUI.indentLevel = 0;
            EditorGUI.BeginChangeCheck();
            {
                position.width *= 0.5f;
                EditorGUI.LabelField(position, new GUIContent(ofType.RequiredType.Name), EditorStyles.miniBoldLabel);

                // Here we would have to create some sort of delegate
                // To allow blackboard automatically set
                // Or retrive parameter value (and store it inside)

                // Minimal solution?
                // Have dict with propertyName as a key (editor only)
                // Blackboard iterates values of dict
                // gets matching value from itself, sets/retrives (type check)

                List<GUIContent> guiContent = new List<GUIContent>();
                foreach (Variant parameter in parameters.Where(p => p.HoldType.Type == ofType.RequiredType))
                {
                    guiContent.Add(new GUIContent(parameter.Name));
                }

                int index = -1;
                Variant value;
                if (asTaskNode.BlackboardRequired.TryGetValue(property.displayName, out value))
                {
                    index = parameters.FindIndex(p => p.Name == value.Name && p.HoldType == value.HoldType);
                }

                position.x += position.width;
                int result = EditorGUI.Popup(position, GUIContent.none, index, guiContent.ToArray(), EditorStyles.objectField);

                if (result >= 0 && result < parameters.Count)
                {
                    var parameter = parameters[result];
                    asTaskNode.BlackboardRequired[property.displayName] = parameter;
                }

                /List<GUIContent> guiContent = new List<GUIContent>();
                foreach (Variant parameter in parameters.Where(p => p.Type == ofType.RequiredType))
                {
                    guiContent.Add(new GUIContent(parameter.Name));
                }

                int index = -1;
                BehaviourTreeNode.RequiredParameter value;
                if (asTaskNode.RequiredParameters.TryGetValue(property.displayName, out value))
                {
                    index = parameters.FindIndex(p => p.Name == value.Name && p.HoldType == value.HoldType);
                }

                position.x += position.width;
                int result = EditorGUI.Popup(position, GUIContent.none, index, guiContent.ToArray(), EditorStyles.objectField);

                if (result >= 0 && result < parameters.Count)
                {
                    var parameter = parameters[result];
                    asTaskNode.RequiredParameters[property.displayName] = parameter;
                }/

                *List<GUIContent> guiContent = new List<GUIContent>();
                foreach (Variant parameter in parameters)
                {
                    guiContent.Add(new GUIContent(string.Format("{0} ({1})", parameter.Name, parameter.HoldType)));
                }

                int index = -1;
                Variant value;
                if (asTaskNode.BlackboardRequired.TryGetValue(property.displayName, out value))
                {
                    index = parameters.FindIndex(p => p.Name == value.Name && p.HoldType == value.HoldType);
                }

                int result = EditorGUI.Popup(position, GUIContent.none, index, guiContent.ToArray(), EditorStyles.objectField);

                if (result >= 0 && result < parameters.Count)
                {
                    var parameter = parameters[result];
                    asTaskNode.BlackboardRequired[property.displayName] = parameter;
                }/
            }
            if (EditorGUI.EndChangeCheck())
            {

            }
        }
        EditorGUI.EndProperty();
    }*/
}
