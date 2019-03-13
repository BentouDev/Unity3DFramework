using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Editor
{
    public class ParametrizedView
    {
        private static readonly int LockSize = 16;
        private static readonly int BoxBackgroundHeight = 4;
        private static readonly int BoxBackgroundMargin = 2;
        internal static readonly int FieldHeight = 22;
        private static readonly Color DarkRed = new Color(128, 0, 0);

        public List<GenericParameter> Parameters { get; set; }

        private List<GUIContent> ParameterListContent { get; set; }

        public IDataSetProvider DataProvider { get; set; }

        public IBaseObject AsParametrized { get; set; }

        public Object Target { get; set; }

        public string Typename { get; set; }

        private void DrawBackground(Rect position, bool isSet)
        {
            var boxRect = position;
            boxRect.height += BoxBackgroundHeight;

            if (!isSet)
            {
                GUI.color = Color.Lerp(DarkRed, Color.clear, 0.5f);
            }
            else
            {
                GUI.color = Color.Lerp(Color.white, new Color(1,1,1,0), 0.5f);
            }
        
            GUI.Box(boxRect, GUIContent.none, SpaceEditorStyles.LightBox);
            GUI.color = Color.white;
        }

        private void SetContentForParameters()
        {
            if (ParameterListContent == null)
                ParameterListContent = new List<GUIContent>();

            ParameterListContent.Clear();

            ParameterListContent.Add(new GUIContent($"none ({Typename})"));

            foreach (GenericParameter parameter in Parameters)
            {
                ParameterListContent.Add(new GUIContent(parameter.Name));
            }
        }

        private void DrawParamField(ref Rect fieldRect, ref int index, SerializedProperty property, bool modified = false)
        {
            fieldRect.y += BoxBackgroundMargin;
            int result = EditorGUI.Popup
            (
                fieldRect,
                GUIContent.none,
                index + 1,
                ParameterListContent.ToArray(), 
                SpaceEditorStyles.ParametrizedField
            );

            bool changed = (result - 1) != index || modified;
            if (changed && DataProvider.CanEditObject(Target))
            {
                if (result > 0 && result <= Parameters.Count)
                {
                    var parameter = Parameters[result - 1];

                    AsParametrized.SetParameter(property.name, parameter);
                }
                else
                {
                    AsParametrized.ClearParameter(property.name);
                }
            }
        }

        public void OnGUI(Rect position, SerializedProperty property, GUIContent label, 
            SerializedType fieldType, PropertyAttribute attribute, bool displayLock = true)
        {
            SetContentForParameters();

            int index = -1;
            bool wasConstant = false;
            var setParam = AsParametrized.GetParameter(property.name, fieldType);
            if (setParam != null)
            {
                index = Parameters.FindIndex(p => p.Name.Equals(setParam.Name) && p.HoldType.Equals(setParam.HoldType));
                wasConstant = AsParametrized.IsParameterConstant(property.name);
            }

            DrawBackground(position, index != -1 || wasConstant);

            label = EditorGUI.BeginProperty(position, label, property);
            {
                position.y += BoxBackgroundHeight * 0.5f;
                position    = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));

                EditorGUI.indentLevel = 0;
                EditorGUI.BeginChangeCheck();
                {
                    var fieldRect = position;

                    if (displayLock)
                    {
                        var lockRect = new Rect(fieldRect.x + fieldRect.width - LockSize, fieldRect.y + 2, LockSize,
                            fieldRect.height);

                        bool isConstant = EditorGUI.Toggle(lockRect, wasConstant, SpaceEditorStyles.LockButton);

                        fieldRect.width -= LockSize;

                        if (isConstant)
                        {
                            GenericParameter parameter = wasConstant
                                ? AsParametrized.GetParameter(property.name, fieldType)
                                : new GenericParameter(fieldType);

                            GenericParamUtils.DrawParameter(fieldRect, parameter, false);

                            AsParametrized.SetParameter(property.name, parameter, true);
                        }
                        else
                        {
                            DrawParamField(ref fieldRect, ref index, property, wasConstant != isConstant);
                        }
                    }
                    else
                    {
                        DrawParamField(ref fieldRect, ref index, property);
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(Target);
                }
            }
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(Parametrized))]
    public class ParametrizedDrawer : PropertyDrawer
    {
        private readonly ParametrizedView View = new ParametrizedView();
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ParametrizedView.FieldHeight;
        }

        private bool Initialize(ref Rect position, SerializedProperty property, GUIContent label, 
            FieldInfo fieldInfo, PropertyAttribute attribute)
        {
            position.height = EditorGUI.GetPropertyHeight(property, label);

            View.Target = property.serializedObject.targetObject;

            View.AsParametrized = View.Target as IBaseObject;

            View.DataProvider = View.AsParametrized?.GetProvider();

            if (View.DataProvider == null)
            {
                EditorGUI.HelpBox(position, $"{property.name}: Unable to get instance of IDataSetProvider!", MessageType.Error);
                return false;
            }

            View.Parameters = View.DataProvider.GetParameters((p) => fieldInfo.FieldType.IsAssignableFrom(p.HoldType.Type));

            View.Typename = KnownType.GetDisplayedName(fieldInfo.FieldType);
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

            if (!(attribute is Parametrized))
            {
                EditorGUI.HelpBox(position, "Unable to get Parametrized attribute!", MessageType.Error);
                return false;
            }

            return true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!Initialize(ref position, property, label, fieldInfo, attribute))
                return;

            View.OnGUI(position, property, label, new SerializedType(fieldInfo.FieldType), attribute);
        }
    }
}