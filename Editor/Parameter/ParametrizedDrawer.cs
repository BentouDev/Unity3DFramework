using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    [CustomPropertyDrawer(typeof(Parametrized))]
    public class ParametrizedDrawer : PropertyDrawer
    {
        private static readonly int LockSize = 16;
        private static readonly int BoxBackgroundHeight = 4;
        private static readonly int BoxBackgroundMargin = 2;
        private static readonly int FieldHeight = 22;
        private static readonly Color DarkRed = new Color(128, 0, 0);

        private List<GenericParameter> Parameters { get; set; }

        private List<GUIContent> ParameterListContent { get; set; }

        private IDataSetProvider DataProvider { get; set; }

        private IBaseObject AsParametrized { get; set; }

        private Object Target { get; set; }

        private string Typename { get; set; }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return FieldHeight;
        }

        private void DrawBackground(Rect position, Color color)
        {
            var boxRect = position;
            boxRect.height += BoxBackgroundHeight;
        
            GUI.color = Color.Lerp(color, Color.clear, 0.5f);
            GUI.Box(boxRect, GUIContent.none);
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

        private bool Initialize(ref Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = base.GetPropertyHeight(property, label);

            Target = property.serializedObject.targetObject;

            AsParametrized = Target as IBaseObject;

            DataProvider = AsParametrized?.GetProvider();

            if (DataProvider == null)
            {
                EditorGUI.HelpBox(position, $"{property.name}: Unable to get instance of IDataSetProvider!", MessageType.Error);
                return false;
            }

            Parameters = DataProvider.GetParameters(p => p.HoldType.Type.IsSubclassOf(fieldInfo.FieldType));

            Typename = KnownType.GetDisplayedName(fieldInfo.FieldType);
            if (Typename == null)
            {
                EditorGUI.HelpBox(position, string.Format("Type {0} is not a known type!", fieldInfo.FieldType), MessageType.Error);
                return false;
            }

            if (!DataProvider.HasObject(Target))
            {
                EditorGUI.HelpBox(position, "Unable to edit this object!", MessageType.Error);
                return false;
            }

            if (!(attribute is Parametrized))
            {
                EditorGUI.HelpBox(position, "Unable to get Parametrized attribute!", MessageType.Error);
                return false;
            }

            SetContentForParameters();

            return true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!Initialize(ref position, property, label))
                return;

            var type = new SerializedType(fieldInfo.FieldType);

            int index = -1;
            bool wasConstant = false;
            var setParam = AsParametrized.GetParameter(property.name, type);
            if (setParam != null)
            {
                index = Parameters.FindIndex(p => p.Name.Equals(setParam.Name) && p.HoldType.Equals(setParam.HoldType));
                wasConstant = AsParametrized.IsParameterConstant(property.name);
            }

            DrawBackground(position, index != -1 || wasConstant ? Color.white : DarkRed);

            label = EditorGUI.BeginProperty(position, label, property);
            {
                position.y += BoxBackgroundHeight * 0.5f;
                position    = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));

                EditorGUI.indentLevel = 0;
                EditorGUI.BeginChangeCheck();
                {
                    //var objectFieldRect = position;

                    var fieldRect = position;//EditorGUILayout.GetControlRect(true, LockSize, SpaceEditorStyles.LightObjectField);
                    var lockRect  = new Rect(fieldRect.x + fieldRect.width - LockSize, fieldRect.y + 2, LockSize, fieldRect.height);

                    bool isConstant = EditorGUI.Toggle(lockRect, wasConstant, SpaceEditorStyles.LockButton);

                    fieldRect.width -= LockSize;

                    if (isConstant)
                    {
                        GenericParameter parameter = wasConstant
                            ? AsParametrized.GetParameter(property.name, type)
                            : new GenericParameter(type);

                        GenericParamUtils.DrawParameter(fieldRect, parameter, false);

                        AsParametrized.SetParameter(property.name, parameter, true);
                    }
                    else
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

                        if ((result - 1) != index && DataProvider.CanEditObject(Target))
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
                }
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(Target);
                }
            }
            EditorGUI.EndProperty();
        }
    }
}