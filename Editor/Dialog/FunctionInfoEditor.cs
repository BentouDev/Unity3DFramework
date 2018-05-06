using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    [CustomPropertyDrawer(typeof(DialogInstance.FunctionInfo))]
    public class FunctionInfoEditor : PropertyDrawer
    {
        private bool Foldout;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var func = property.FindPropertyRelative("Function");
            string name = property.FindPropertyRelative("Name").stringValue;
            //func.isExpanded = EditorGUI.Foldout(position, func.isExpanded, new GUIContent(name));
            //if (func.isExpanded)
            {
                EditorGUI.PropertyField(position, func, new GUIContent(name));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return base.GetPropertyHeight(property, label);

            float height = 72;

            var prop = property.FindPropertyRelative("FunctionCount");
            if (prop != null)
            {
                int count = prop.intValue;
                height += Mathf.Max(0, count - 1) * 43;
            }

            return height;
        }
    }
}
