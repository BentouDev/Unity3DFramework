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
            string name = property.FindPropertyRelative("Name").stringValue;
            Foldout = EditorGUI.Foldout(position, Foldout, new GUIContent(name));
            if (Foldout)
            {
                EditorGUI.PropertyField(position, property.FindPropertyRelative("Function"), new GUIContent(name));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!Foldout)
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
