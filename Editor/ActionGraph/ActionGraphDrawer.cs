using System.Linq;
using Boo.Lang;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Framework.Editor
{
    [CustomPropertyDrawer(typeof(ActionGraph))]
    public class ActionGraphInstanceDrawer : PropertyDrawer
    {
        private ActionGraph Graph;
        private IActionGraphOwner Owner;
        private readonly ParameterBindView BindView = new ParameterBindView();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Owner = property.serializedObject.targetObject as IActionGraphOwner;
            Graph = property.objectReferenceValue as ActionGraph;
            
            if (Owner == null)
                return EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded || property.objectReferenceValue == null)
                return base.GetPropertyHeight(property, label);

            return Graph ? BindView.GetHeight() : 38;
        }

        private Rect HandleLabel(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue != null)
                return EditorGUI.PrefixLabel(position, label, SpaceEditorStyles.InvisibleText);
            return EditorGUI.PrefixLabel(position, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Owner = property.serializedObject.targetObject as IActionGraphOwner;
            Graph = property.objectReferenceValue as ActionGraph;

            if (Owner == null)
            {
                EditorGUI.HelpBox(position,
                    $"type '{property.serializedObject.targetObject.GetType().Name}' must derive from IActionGraphOwner!", 
                    MessageType.Error);
                return;
            }

            if (Graph != null)
            {
                BindView.Initialize(Graph.Parameters, Owner.GetBindingContext(Graph), property.serializedObject.targetObject);
            }

            var oldRect = position;

            position.y -= 2;
            position.width += position.x + 4;
            position.x = 0;
            position.height  = EditorGUIUtility.singleLineHeight;
            position.height += 4;

            GUI.color = Color.Lerp(Color.white, Color.clear, 0.35f);
            GUI.Box(position, GUIContent.none, SpaceEditorStyles.LightBox);
            GUI.color = Color.white;

            position.width = oldRect.width;
            position.y += 2;
            position.x = oldRect.x;

            position = HandleLabel(position, property, label);
            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.ObjectField(position, property, typeof(ActionGraph), GUIContent.none);

            if (property.objectReferenceValue != null)
            {
                var foldRect = oldRect;
                foldRect.height = EditorGUIUtility.singleLineHeight;
                property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, label, true);            
                if (property.isExpanded)
                {
                    position = oldRect;
                    position.x = 0;
                    position.y += EditorGUIUtility.singleLineHeight + 2;
                    position.height -= EditorGUIUtility.singleLineHeight;
                    position.width += 20;

                    GUI.color = Color.Lerp(Color.white, Color.black, 0.65f);
                    GUI.Box(position, GUIContent.none);

                    EditorGUI.BeginChangeCheck();
                    {
                        BindView.DoList(position);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        var component = (property.serializedObject.targetObject as MonoBehaviour);
                        if (component)
                        {
                            if (component.gameObject.scene.IsValid())
                                EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                            else
                                EditorUtility.SetDirty(component);
                        }
                    }

                    GUI.color = Color.white;
                }
            }
        }
    }
}