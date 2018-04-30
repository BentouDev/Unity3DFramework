using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    [CustomPropertyDrawer(typeof(DialogInstance.ActorInfo))]
    public class ActorInfoEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);

            float oldWidth = contentPosition.width;
            contentPosition.width *= 0.25f;

            var type = property.FindPropertyRelative("Type");

            EditorGUI.PropertyField(contentPosition, type, GUIContent.none);

            contentPosition.x += contentPosition.width;
            contentPosition.width = oldWidth - contentPosition.width;

            if (type.enumValueIndex == (int) DialogInstance.ActorType.Dynamic)
            {
                EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("Tag"), GUIContent.none);
            }
            else if (type.enumValueIndex == (int)DialogInstance.ActorType.Static)
            {
                EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("Actor"), GUIContent.none);
            }
            else
            {
                EditorGUI.LabelField(contentPosition, property.serializedObject.targetObject.name);
            }
        }
    }
}
