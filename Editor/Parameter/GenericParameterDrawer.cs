using System;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Framework.Editor.Parameter
{
    [CustomPropertyDrawer(typeof(GenericParameter))]
    public class GenericParameterDrawer : PropertyDrawer
    {
        private ParametrizedView View = new ParametrizedView();

        bool Initialize(ref Rect position, ref bool displayLock, SerializedProperty property, GenericParameter asGeneric)
        {
            Type targetType = property.serializedObject.targetObject.GetType();
            SerializedType isTypeRestricted = null;

            var path = property.GetPath();
            FieldInfo paramFieldInfo = path.MaterializeToFieldInfo(targetType);

            var typeAttribute = paramFieldInfo.GetCustomAttribute<TypeRestricted>();
            if (typeAttribute != null)
            {
                switch (typeAttribute.Source)
                {
                    case TypeRestricted.TypeSource.Value:
                    {
                        isTypeRestricted = new SerializedType(typeAttribute.Type);
                        break;
                    }
                    case TypeRestricted.TypeSource.Field:
                    {
                        var field = targetType.GetField(typeAttribute.SourceValue);
                        if (field != null)
                        {
                            var typeValue = field.GetValue(property.serializedObject.targetObject);
                            switch (typeValue)
                            {
                                case SerializedType asSerialized:
                                {
                                    isTypeRestricted = asSerialized;
                                    break;
                                }
                                case Type asSystem:
                                {
                                    isTypeRestricted = new SerializedType(asSystem);
                                    break;
                                }
                            }
                            break;
                        }

                        return false;
                    }
                    default:
                        return false;
                }
            }

            View.Target = property.serializedObject.targetObject;
            View.AsParametrized = View.Target as IBaseObject;
            View.DataProvider = View.AsParametrized?.GetProvider();

            if (isTypeRestricted != null && isTypeRestricted.Type != null)
            {
                displayLock = true;
                View.Parameters = View.DataProvider.GetParameters(t =>
                {
                    if (string.IsNullOrEmpty(isTypeRestricted.Metadata))
                        return isTypeRestricted.Type.IsAssignableFrom(t.HoldType.Type);
                    else
                        return isTypeRestricted.Equals(t.HoldType);
                });
                View.Typename = KnownType.GetDisplayedName(isTypeRestricted.Type);
            }
            else
            {
                displayLock = false;
                View.Parameters = View.DataProvider.GetParameters();
                View.Typename = "Parameter";
            }

            if (asGeneric.HoldType != isTypeRestricted)
            {
                asGeneric.HoldType = isTypeRestricted;
            }

            if (View.Typename == null)
            {
                EditorGUI.HelpBox(position, string.Format("Type {0} is not a known type!", fieldInfo.FieldType), MessageType.Error);
                return false;
            }

            if (!View.DataProvider.HasObject(View.Target))
            {
                EditorGUI.HelpBox(position, "Unable to edit this object!", MessageType.Error);
                return false;
            }

            if (View.DataProvider == null)
            {
                EditorGUI.HelpBox(position, $"{property.name}: Unable to get instance of IDataSetProvider!", MessageType.Error);
                return false;
            }

            return true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ParametrizedView.FieldHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUI.GetPropertyHeight(property, label);

            var asGeneric = property.GetAs<GenericParameter>();
            var asBaseObject = property.serializedObject.targetObject as IBaseObject;

            bool displayLock = true;

            IDataSetProvider provider = asBaseObject?.GetProvider();
            if (asGeneric != null && provider != null && Initialize(ref position, ref displayLock, property, asGeneric))
            {
                View.OnGUI(position, property, label, asGeneric.HoldType, attribute, displayLock);
            }
            else
            {
                EditorGUI.HelpBox(position, "Cannot draw as GenericParameter!", MessageType.Error);
            }
        }
    }
}