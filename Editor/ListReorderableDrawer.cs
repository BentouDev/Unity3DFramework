using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Framework.Editor
{
    [CustomPropertyDrawer(typeof(ListReordeable))]
    public class ListReorderableDrawer : PropertyDrawer
    {
        private UnityEditorInternal.ReorderableList List;
        private List<SerializedProperty> Proxy;
        private SerializedProperty LastProp;
        
        void Initialize()
        {
            if (List != null)
                return;
            
            if (Proxy != null)
                Proxy.Clear();
            else
            {
                Proxy = new List<SerializedProperty>();
            }
            
            List = new UnityEditorInternal.ReorderableList(Proxy, Proxy.GetType())
            {
                drawElementCallback = (rect, index, active, focused) =>
                {
                    EditorGUI.PropertyField(rect, Proxy[index], new GUIContent(index.ToString()), false);
                },
                onAddCallback = list =>
                {
                    if (LastProp == null)
                        return;
                    
                    LastProp.arraySize++;
                },
                onRemoveCallback = list =>
                {
                    if (LastProp == null)
                        return;

                    Proxy.RemoveAt(list.index);
                    LastProp.DeleteArrayElementAtIndex(list.index);
                }
            };
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!property.isArray)
            {
                EditorGUI.HelpBox(position, $"Cannot use ListReorderable on {property.name} because its not an array!", MessageType.Error);
                return;
            }
            
            Initialize();
 
            for (int i = 0; i < property.arraySize; i++)
            {
                Proxy.Add(property.GetArrayElementAtIndex(i));
            }

            LastProp = property;
            List.DoList(position);
        }
    }
}