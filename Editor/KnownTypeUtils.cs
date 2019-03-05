using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    public static class KnownTypeUtils
    {
        public static void ShowAddParameterMenu(System.Action<SerializedType> callback, bool addNulltype = false)
        {
            GenericMenu menu = new GenericMenu();

            if (addNulltype)
            {
                menu.AddItem(new GUIContent("none"), false, () => callback(null));
            }

            var types = KnownType.GetKnownTypes();
            for (int i = 0; i < types.Count; i++)
            {
                var type = types[i];
                menu.AddItem(new GUIContent(type.GenericName), false, CreateNewParameterCallback(type.HoldType, callback));
            }

            menu.ShowAsContext();
        }

        static UnityEditor.GenericMenu.MenuFunction CreateNewParameterCallback(System.Type type, System.Action<SerializedType> callback)
        {
            return () =>
            {
                OnAddNewParameter(type, callback);
            };
        }

        private static void OnAddNewParameter(System.Type type, System.Action<SerializedType> callback)
        {
            if (type == typeof(DerivedType))
            {
                ReferenceTypePicker.ShowWindow(KnownType.ObjectType, t => callback(new SerializedType(type, t.AssemblyQualifiedName)), t => t.IsSubclassOf(KnownType.ObjectType));
            }
            else if(type.IsSubclassOf(typeof(UnityEngine.Object)) && type != typeof(GameObject))
            {
                ReferenceTypePicker.ShowWindow(type, t => callback(new SerializedType(t)), t => t.IsSubclassOf(type));
            }
            else
            {
                callback(new SerializedType(type));
            }
        }
    }
}