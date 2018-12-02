using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    [CustomPropertyDrawer(typeof(DataSetInstance.DataLayer))]
    public class DataSetLayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeEnum = property.FindPropertyRelative("Type");

            var oldWidth = position.width;
            position.width *= 0.35f;

            EditorGUI.PropertyField(position, typeEnum, GUIContent.none);

            position.x += position.width;
            position.width = oldWidth - position.width;

            var enumValue = (DataSetInstance.Type) typeEnum.enumValueIndex;
            switch (enumValue)
            {
                case DataSetInstance.Type.Instance:
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("LayeredSet"), GUIContent.none);
                    break;
                case DataSetInstance.Type.Scene:
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("SceneSet"), GUIContent.none);
                    break;
                case DataSetInstance.Type.Asset:
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("AssetSet"), GUIContent.none);
                    break;
            }
        }
    }
}