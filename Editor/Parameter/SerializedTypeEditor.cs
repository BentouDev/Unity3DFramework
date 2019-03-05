using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Framework.Editor
{
    [CustomPropertyDrawer(typeof(SerializedType))]
    public class SerializedTypeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var target = property.GetAs<SerializedType>();
            string content = target.ToString();

            position.height = base.GetPropertyHeight(property, label);
            position = EditorGUI.PrefixLabel(position, label);

            if (GUI.Button(position, content, EditorStyles.objectField))
            {
                KnownTypeUtils.ShowAddParameterMenu(type =>
                {
                    target.Type = type?.Type;
                    target.Metadata = type?.Metadata;
                }, true);
            }
        }
    }
}