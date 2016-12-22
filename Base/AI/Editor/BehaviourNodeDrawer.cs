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
    private static readonly int BoxBackgroundHeight = 4;
    private static readonly int BoxBackgroundMargin = 2;
    private static readonly int FieldHeight = 22;

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
        
        var requiredParameters = Node.GetType().GetProperties().Where(p => System.Attribute.IsDefined(p, typeof(Blackboard.Required)));
        foreach (PropertyInfo propertyInfo in requiredParameters)
        {
            if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
            {
                EditorGUILayout.HelpBox(string.Format("Property '{0}' must be both readable and writable", propertyInfo.Name), MessageType.Error);
                continue;
            }

            var typename = GenericParameter.GetDisplayedName(propertyInfo.PropertyType);
            if (typename == null)
            {
                EditorGUILayout.HelpBox(string.Format("Type {0} is not a known type!", propertyInfo.PropertyType), MessageType.Error);
                continue;
            }

            var matchingParameters = Parameters.Where(p => p.HoldType.Type == propertyInfo.PropertyType).ToList();

            int index = Node.GetGenericParameterIndex(propertyInfo.Name, propertyInfo.PropertyType, matchingParameters);

            var paramListContent = new List<GUIContent>(matchingParameters.Count() + 1);
                
            paramListContent.Add(new GUIContent(string.Format("none ({0})", typename)));

            foreach (GenericParameter parameter in matchingParameters)
            {
                paramListContent.Add(new GUIContent(parameter.Name));
            }

            GUI.color = index == -1 ? Color.red : Color.white;
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(FieldHeight));
            {
                GUI.color = Color.white;
                GUILayout.BeginHorizontal();//GUILayout.ExpandWidth(true), GUILayout.Height(FieldHeight));
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.PrefixLabel(new GUIContent(propertyInfo.Name));
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndVertical();
                    
                    int result = EditorGUILayout.Popup(GUIContent.none, index + 1, paramListContent.ToArray(), (GUIStyle)"ShurikenObjectField");

                    if (!Editor.ExecuteInRuntime())
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
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        if(EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}
