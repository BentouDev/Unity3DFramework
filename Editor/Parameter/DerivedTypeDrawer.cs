using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Parameter
{
    [CustomPropertyDrawer(typeof(DerivedType))]
    public class DerivedTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var type = property.GetAs<DerivedType>();
            if (type == null)
                return;

            position.width *= 0.5f;

            string baseContent = type.BaseType != null && type.BaseType.Type != null 
                ? $"{type.BaseType.Type.Name} (System.Type)" : "None (System.Type)";
            
            if (EditorGUI.DropdownButton(position, new GUIContent(baseContent), FocusType.Keyboard))
            {
                KnownTypeUtils.ShowAddParameterMenu(t =>
                {
                    if (t != type.BaseType)
                    {
                        type.BaseType = t;
                        type.TypeValue = null;
                    }
                });
            }

            position.x += position.width;

            string derivedContent = type.DisplayedName;
            if (EditorGUI.DropdownButton(position, new GUIContent(derivedContent), FocusType.Keyboard))
            {
                ReferenceTypePicker.ShowWindow(type.BaseType.Type,
                    t => { type.TypeValue = new SerializedType(t); },
                    t => t.IsSubclassOf(type.BaseType.Type));
            }
        }
    }
}