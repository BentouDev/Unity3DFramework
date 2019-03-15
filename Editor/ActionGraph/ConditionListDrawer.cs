using System;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    [CustomPropertyDrawer(typeof(TConditionList))]
    public class ConditionListDrawer : PropertyDrawer
    {
        private SerializedProperty CurrentProperty;
        private TConditionList Target;

        ReorderableList GetList(SerializedProperty property)
        {
            var (initialize, list) = ReorderableDrawer.GetList(property);
            if (initialize)
            {
                list.drawElementCallback += DrawCondition;
                list.onAddDropdownCallback += AddCondition;
                list.getElementHeightCallback += GetConditionHeight;
                list.onRemoveCallback += OnConditionRemoved;
            }

            return list;
        }

        private void OnConditionRemoved(int[] list)
        {
            foreach (var index in list)
            {
                Target[index].Graph = null;
                Target.RemoveAt(index);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CurrentProperty = property;
            Target = CurrentProperty.GetAs<TConditionList>();

            // ToDo: Hack!
            foreach (var cond in Target)
            {
                cond.OnSetupParametrizedProperties();
            }

            GUI.color = Color.Lerp(Color.white, Color.black, 0.25f);
            
            var list = GetList(property);
            list.DoList(position, label);

            GUI.color = Color.white;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetList(property).GetHeight();
        }

        private void AddCondition(Rect buttonrect, ReorderableList list)
        {
            ReferenceTypePicker.ShowWindow(typeof(Condition),
                t =>
                {
                    OnConditionAdded(list, t);
                },
                t => t.IsSubclassOf(typeof(Condition)));
        }

        private void OnConditionAdded(ReorderableList list, Type type)
        {
            var instance = ScriptableObject.CreateInstance(type) as Condition;
            list.ListUnsafe.GetAs<TConditionList>()?.Add(instance);
            AssetDatabaseUtils.AddToParentAsset(list.List.serializedObject.targetObject,instance);

            // Set Graph reference
            var path = AssetDatabase.GetAssetPath(list.List.serializedObject.targetObject);
            if (!string.IsNullOrEmpty(path))
            {
                var parent = AssetDatabase.LoadAssetAtPath(path, typeof(ActionGraph));
                instance.Graph = parent as ActionGraph;
            }            
        }

        private float GetConditionHeight(SerializedProperty property, int index)
        {
            if (property.isExpanded && property.objectReferenceValue != null)
            {
                var condition = property.GetAs<Condition>();
                return EditorGUIUtility.singleLineHeight + InspectorUtils.GetDefaultInspectorHeight(new SerializedObject(condition));
            }

            return VariantUtils.FieldHeight;
        }

        private void DrawCondition(Rect rect, SerializedProperty element, GUIContent label, int index, bool selected, bool focused)
        {
            Condition condition = null;
            string description = string.Empty;
            if (element.objectReferenceValue != null)
            {
                condition = element.GetAs<Condition>();
                description = condition.GetDescription();
            }

            rect.x += 10;
            rect.width -= 10;
            var foldoutRect = rect;
            foldoutRect.height = VariantUtils.FieldHeight;

            var backgroundRect = rect;
            backgroundRect.x -= 29;
            backgroundRect.y -= 1;
            backgroundRect.width += 33;
            backgroundRect.height = 20;

            var oldColor = GUI.color;
            GUI.color = Color.Lerp(new Color(1,1,1,0), Color.white, 0.75f);
            GUI.Box(backgroundRect, GUIContent.none, SpaceEditorStyles.LightBox);
            GUI.color = oldColor;

            element.isExpanded = EditorGUI.Foldout(foldoutRect, element.isExpanded, new GUIContent(description), true);
            if (element.isExpanded)
            {
                rect.y += VariantUtils.FieldHeight + 4;
                rect.height -= VariantUtils.FieldHeight + 2;

                if (condition)
                {
                    if (InspectorUtils.DrawDefaultInspector(rect, new SerializedObject(condition)))
                    {
                        if (element.serializedObject != null 
                        &&  element.serializedObject.targetObject != null)
                            EditorUtility.SetDirty(element.serializedObject.targetObject);
                    }
                }

                {
                    /*GUILayout.BeginArea(rect);

                    if (InspectorUtils.DrawDefaultInspectorWithoutScriptField(new SerializedObject(condition)))
                    {
                        EditorUtility.SetDirty(CurrentProperty.serializedObject.targetObject);
                    }

                    GUILayout.EndArea();*/
                }
            }
        }
    }
}