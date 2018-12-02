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
    private static readonly int BoxBackgroundHeight = 4;
    private static readonly int BoxBackgroundMargin = 2;
    private static readonly int FieldHeight = 22;

    private List<GenericParameter> Parameters { get; set; }

    private BehaviourTreeNode AsNode { get; set; }

    private BehaviourTreeEditor Editor { get; set; }

    private List<GUIContent> ParameterListContent { get; set; }

    private string Typename { get; set; }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return FieldHeight;
    }

    private void DrawBackground(Rect position, Color color)
    {
        var boxRect = position;
            boxRect.height += BoxBackgroundHeight;
        
        GUI.color = color;
        GUI.Box(boxRect, GUIContent.none);
        GUI.color = Color.white;
    }
    
    private void SetContentForParameters()
    {
        if(ParameterListContent == null)
            ParameterListContent = new List<GUIContent>();

        ParameterListContent.Clear();

        ParameterListContent.Add(new GUIContent(string.Format("none ({0})", Typename)));

        foreach (GenericParameter parameter in Parameters)
        {
            ParameterListContent.Add(new GUIContent(parameter.Name));
        }
    }

    private bool Initialize(ref Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = base.GetPropertyHeight(property, label);

        Editor = BehaviourTreeEditor.GetInstance();
        if (!Editor)
        {
            EditorGUI.HelpBox(position, "Unable to get BehaviourTreeEditor instance!", MessageType.Error);
            return false;
        }

        Parameters = Editor.GetCurrentAsset().Parameters.Where(p => p.HoldType.Type == fieldInfo.FieldType).ToList();

        AsNode = (BehaviourTreeNode) property.serializedObject.targetObject;

        Typename = GenericParameter.GetDisplayedName(fieldInfo.FieldType);
        if (Typename == null)
        {
            EditorGUI.HelpBox(position, string.Format("Type {0} is not a known type!", fieldInfo.FieldType), MessageType.Error);
            return false;
        }
        
        if (!Editor.CanEditNode(AsNode))
        {
            EditorGUI.HelpBox(position, "Unable to edit this node!", MessageType.Error);
            return false;
        }

        if (!(attribute is Blackboard.Required))
        {
            EditorGUI.HelpBox(position, "Unable to get Required attribute!", MessageType.Error);
            return false;
        }

        SetContentForParameters();

        return true;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!Initialize(ref position, property, label))
            return;

        int index = AsNode.GetGenericParameterIndex(property.name, fieldInfo.FieldType, Parameters);

        DrawBackground(position, index == -1 ? Color.red : Color.white);

        label = EditorGUI.BeginProperty(position, label, property);
        {
            position.y += BoxBackgroundHeight * 0.5f;
            position    = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));

            EditorGUI.indentLevel = 0;
            EditorGUI.BeginChangeCheck();
            {
                var objectFieldRect = position;
                    objectFieldRect.y += BoxBackgroundMargin;

                int result = EditorGUI.Popup
                (
                    objectFieldRect, 
                    GUIContent.none, 
                    index + 1, 
                    ParameterListContent.ToArray(), 
                    SpaceEditorStyles.ParametrizedField
                );

                if (!Editor.ExecuteInRuntime())
                {
                    if (result > 0 && result <= Parameters.Count)
                    {
                        var parameter = Parameters[result - 1];

                        AsNode.SetRequiredParameter(property.name, parameter);
                    }
                    else
                    {
                        AsNode.ClearRequiredParamerer(property.name);
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {

            }
        }
        EditorGUI.EndProperty();
    }
}
