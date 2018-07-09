using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Base
{
    [CustomPropertyDrawer(typeof(AsReorderableList))]
    public class ReorderableListDrawer : PropertyDrawer
    {
        private UnityEditorInternal.ReorderableList List;

        private bool Initialize(SerializedProperty property)
        {
            if (property == null)
                return false;

            if (!property.isArray || property.serializedObject == null)
                return false;
            
            if (List == null)
                List = new UnityEditorInternal.ReorderableList(property.serializedObject, property);

            List.displayRemove = true;
            List.displayAdd    = true;
            List.draggable     = true;
            List.drawHeaderCallback = rect => EditorGUI.PrefixLabel(rect, new GUIContent(property.name));
            List.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.x += 20;
                rect.y += 2;
                var element = property.GetArrayElementAtIndex(index);
                EditorGUI.BeginProperty(rect, new GUIContent(element.displayName), element);
                {
                    EditorGUI.PropertyField(rect, element, true);                    
                }
                EditorGUI.EndProperty();
            };

            List.elementHeightCallback = index => { 
                var element = property.GetArrayElementAtIndex(index);
                if (element.isExpanded) 
                    return 44;
                return 22;
            };
            
            return true;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!Initialize(property))
            {
                EditorGUI.HelpBox(position, "Unable to use 'ReorderableList' for " + property.name, MessageType.Error);
                return;
            }
            
            List.DoList(position);
        }
    }
}