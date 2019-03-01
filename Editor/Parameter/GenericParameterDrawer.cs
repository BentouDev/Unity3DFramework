using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Parameter
{
    [CustomPropertyDrawer(typeof(GenericParameter))]
    public class GenericParameterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var asGeneric = property.GetAs<GenericParameter>();
            if (asGeneric != null)
            {
                GenericParameter.Draw(position, asGeneric, true);
            }
            else
            {
                EditorGUI.HelpBox(position, "Cannot draw as GenericParameter!", MessageType.Error);
            }
        }
    }
}