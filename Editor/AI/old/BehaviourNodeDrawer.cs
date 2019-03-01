using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework;
using Framework.AI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BehaviourTreeNode), true)]
public class BehaviourNodeDrawer : Editor
{
    private static readonly int FieldHeight = 22;
    private static readonly int FieldLeftShift = 32;
    private static readonly int LockSize = 16;

    public BehaviourTreeNode Node
    {
        get { return (BehaviourTreeNode)target; }
    }

    private BehaviourTreeEditor Editor { get; set; }

    private List<GenericParameter> Parameters { get; set; }

    private bool Initialize()
    {
        Editor = BehaviourTreeEditor.GetInstance();
        if (!Editor)
        {
            EditorGUILayout.HelpBox("Unable to get BehaviourTreeEditor instance!", MessageType.Error);
            return false;
        }

        Parameters = Editor.GetCurrentAsset().Parameters;
        
        if (!Editor.CanEditNode(Node))
        {
            EditorGUILayout.HelpBox("Unable to edit this node!", MessageType.Error);
            return false;
        }
        
        return true;
    }

    public override void OnInspectorGUI()
    {
        if (!Initialize())
            return;

        InspectorUtils.DrawDefaultScriptField(serializedObject);

        GUILayout.Label("Description", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(Node.Description, EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space();
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        InspectorUtils.DrawDefaultInspectorWithoutScriptField(serializedObject);

        EditorGUILayout.Space();
        GUILayout.Label("Parameters", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        EditorGUIUtility.labelWidth -= FieldLeftShift;
        var requiredParameters = Node.GetType().GetProperties().Where(p => System.Attribute.IsDefined(p, typeof(Blackboard.Required)));
        foreach (PropertyInfo propertyInfo in requiredParameters)
        {
            if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
            {
                EditorGUILayout.HelpBox(string.Format("Property '{0}' must be both readable and writable", propertyInfo.Name), MessageType.Error);
                continue;
            }

            var typename = KnownType.GetDisplayedName(propertyInfo.PropertyType);
            if (typename == null)
            {
                EditorGUILayout.HelpBox(string.Format("Type {0} is not a known type!", propertyInfo.PropertyType), MessageType.Error);
                continue;
            }

            var matchingParameters = Parameters.Where(p => p.HoldType.Type == propertyInfo.PropertyType).ToList();

            int  oldIndex    = Node.GetGenericParameterIndex(propertyInfo.Name, propertyInfo.PropertyType, matchingParameters);
            bool wasConstant = Node.IsGenericParameterConstant(propertyInfo.Name);
            
            BeginPanelBackground(wasConstant, oldIndex == -1);
            { 
                GUILayout.BeginHorizontal();
                {
                    DrawLabel(propertyInfo.Name);

                    var fieldRect = EditorGUILayout.GetControlRect(true, LockSize, SpaceEditorStyles.LightObjectField);
                    var lockRect  = new Rect(fieldRect.x + fieldRect.width - LockSize, fieldRect.y + 2, LockSize, fieldRect.height);

                    bool isConstant = EditorGUI.Toggle(lockRect, wasConstant, SpaceEditorStyles.LockButton);

                    fieldRect.width -= LockSize;
                    
                    if (isConstant)
                    {
                        GenericParameter parameter = wasConstant
                            ? Node.ParametrizedProperties[propertyInfo.Name].Parameter
                            : new GenericParameter(propertyInfo.PropertyType);

                        GenericParamUtils.DrawParameter(fieldRect, parameter, false);

                        Node.SetRequiredParameter(propertyInfo.Name, parameter, true);
                    }
                    else
                    {
                        fieldRect.y += 2;

                        var paramListContent = BuildParameterGUIList(typename, matchingParameters);
                        int newIndex = EditorGUI.Popup(fieldRect, GUIContent.none, oldIndex + 1, paramListContent.ToArray(), SpaceEditorStyles.LightObjectField);

                        if (!Editor.ExecuteInRuntime() && (oldIndex != (newIndex - 1) || (isConstant != wasConstant)))
                        {
                            ProcessSelectionResult(newIndex, propertyInfo, matchingParameters);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        if(EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }

    private void BeginPanelBackground(bool isConstant, bool hasIndex)
    {
        GUI.color = hasIndex ? Color.red : Color.white;
        GUI.color = isConstant ? new Color(1,0.75f,0.125f) : GUI.color;
        
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(FieldHeight));
        
        GUI.color = Color.white;
    }

    private void DrawLabel(string label)
    {
        GUILayout.BeginVertical();
        {
            GUILayout.FlexibleSpace();

            EditorGUILayout.PrefixLabel(new GUIContent(label));

            GUILayout.FlexibleSpace();
        }
        GUILayout.EndVertical();
    }
    
    private List<GUIContent> BuildParameterGUIList(string typename, List<GenericParameter> matchingParameters)
    {
        var paramListContent = new List<GUIContent>(matchingParameters.Count() + 1);

        paramListContent.Add(new GUIContent(string.Format("None ({0})", typename)));

        foreach (GenericParameter parameter in matchingParameters)
        {
            paramListContent.Add(new GUIContent(parameter.Name));
        }
        
        return paramListContent;
    }

    private void ProcessSelectionResult(int result, PropertyInfo propertyInfo, List<GenericParameter> matchingParameters)
    {
        if (result > 0 && result <= matchingParameters.Count)
        {
            var parameter = matchingParameters[result - 1];

            Node.SetRequiredParameter(propertyInfo.Name, parameter);
        }
        else
        {
            Node.ClearRequiredParamerer(propertyInfo.Name);
        }
    }
}
