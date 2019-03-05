using System.Collections;
using System.Collections.Generic;
using Framework.AI;
using Framework.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    [InitializeOnLoad]
    public static class GenericParamUtils
    {
        public static int LabelWidth = 100;
        public static int FieldWidth = 150;
        public static int FieldHeight = 16;

        static GenericParamUtils()
        {
            GenericParameter.BuildKnownTypeList();
            SetDrawersForKnownTypes();
            // Debug.LogFormat("Regenerated GenericParam for : {0} types", KnownType.Register.Count);
        }

        public static void SetDrawersForKnownTypes()
        {
            SetDrawerForKnownType<int>(DrawAsInt, LayoutAsInt);
            SetDrawerForKnownType<bool>(DrawAsBool, LayoutAsBool);
            SetDrawerForKnownType<float>(DrawAsFloat, LayoutAsFloat);
            SetDrawerForKnownType<Color>(DrawAsColor, LayoutAsColor);
            SetDrawerForKnownType<Vector2>(DrawAsVec2, LayoutAsVec2);
            SetDrawerForKnownType<Vector3>(DrawAsVec3, LayoutAsVec3);
            SetDrawerForKnownType<string>(DrawAsString, LayoutAsString);
            SetDrawerForKnownType<AnimationCurve>(DrawAsCurve, LayoutAsCurve);
            SetDrawerForKnownType<RuntimeAnimatorController>(DrawAsObject<RuntimeAnimatorController>, LayoutAsObject<RuntimeAnimatorController>);
            SetDrawerForKnownType<SerializedType>(DrawAsTypePicker, LayoutAsTypePicker);
            SetDrawerForKnownType<Component>(DrawAsObject<Component>, LayoutAsObject<Component>);
            SetDrawerForKnownType<DerivedType>(DrawAsDerivedTypePicker, LayoutAsDerivedTypePicker);
            SetDrawerForKnownType<GameObject>(DrawAsObject<GameObject>, LayoutAsObject<GameObject>);
            SetDrawerForKnownType<ScriptableObject>(DrawAsObject<ScriptableObject>, LayoutAsObject<ScriptableObject>);
        }

        private static void SetDrawerForKnownType<T>(KnownType.TypeDrawFunc draw, KnownType.TypeLayoutFunc layout)
        {
            var typename = typeof(T).FullName;
            
            KnownType info;
            if (KnownType.Register.TryGetValue(typename, out info))
            {
                info.DrawFunc = draw;
                info.LayoutFunc = layout;
            }
            else
            {
                Debug.LogErrorFormat("GenericParameter for type {0} was not found", typename);
            }
        }

        public static void DrawParameter(Rect drawRect, GenericParameter parameter, bool label = true)
        {
            GenericParameter.Draw(drawRect, parameter, label);
        }
        
        public static void LayoutParameter(GenericParameter parameter, bool label = true)
        {
            GenericParameter.Layout(parameter, label);
        }

        private static System.Action<SerializedType> BuildSerializedTypeCallback(GenericParameter param)
        {
            return type =>
            {
                param.SetAs<SerializedType>(type);
            };
        }

        private static System.Action<System.Type> BuildDerivedTypeCallback(GenericParameter param, System.Type baseType)
        {
            return type =>
            {
                param.SetAs<DerivedType>(new DerivedType(baseType, type));
            };
        }
        
        #region Draw

        private static void DrawAsDerivedTypePicker(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width*0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width*0.5f;
                rect.x += rect.width;
            }

            var type = parameter.GetAs<DerivedType>();
            string content = type.DisplayedName;

            if (EditorGUI.DropdownButton(rect, new GUIContent(content), FocusType.Keyboard))
            {
                SerializedType sysType = type.BaseType ?? new SerializedType(parameter.HoldType.Metadata);

                ReferenceTypePicker.ShowWindow(sysType.Type,
                    BuildDerivedTypeCallback(parameter, sysType.Type),
                    t => t.IsSubclassOf(sysType.Type));
            }
        }

        private static void DrawAsTypePicker(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width*0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width*0.5f;
                rect.x += rect.width;
            }

            var type = parameter.GetAs<SerializedType>();
            string content = type.Type != null ? $"{type.Type.Name} (System.Type)" : "None (System.Type)";

            if (EditorGUI.DropdownButton(rect, new GUIContent(content), FocusType.Keyboard))
            {
                KnownTypeUtils.ShowAddParameterMenu(BuildSerializedTypeCallback(parameter));
            }
        }

        private static void DrawAsObject<T>(Rect rect, GenericParameter parameter, bool label) where T : UnityEngine.Object
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width*0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width*0.5f;
                rect.x += rect.width;
            }

            T result = (T) EditorGUI.ObjectField(rect, parameter.GetAs<T>(), parameter.HoldType.Type, true);
            parameter.SetAs(result);
        }

        private static void DrawAsGenericObject(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width * 0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width * 0.5f;
                rect.x += rect.width;
            }

            var result = EditorGUI.ObjectField(rect, parameter.GetAs<Object>(), parameter.HoldType.Type, true);
            parameter.SetAs(result);
        }

        private static void DrawAsColor(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width * 0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width * 0.5f;
                rect.x += rect.width;
            }

            var result = EditorGUI.ColorField(rect, parameter.GetAs<Color>());
            parameter.SetAs(result);
        }

        private static void DrawAsCurve(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width * 0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width * 0.5f;
                rect.x += rect.width;
            }

            var result = EditorGUI.CurveField(rect, parameter.GetAs<AnimationCurve>());
            parameter.SetAs(result);
        }

        private static void DrawAsString(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width * 0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width * 0.5f;
                rect.x += rect.width;
            }

            var result = EditorGUI.TextField(rect, parameter.GetAs<string>());
            parameter.SetAs(result);
        }

        private static void DrawAsVec3(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width * 0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width * 0.5f;
                rect.x += rect.width;
            }

            var result = EditorGUI.Vector3Field(rect, GUIContent.none, parameter.GetAs<Vector3>());
            parameter.SetAs(result);
        }

        private static void DrawAsVec2(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width * 0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width * 0.5f;
                rect.x += rect.width;
            }

            var result = EditorGUI.Vector2Field(rect, GUIContent.none, parameter.GetAs<Vector2>());
            parameter.SetAs(result);
        }

        private static void DrawAsInt(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width * 0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width * 0.5f;
                rect.x += rect.width;
            }

            var oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 13;

            var result = EditorGUI.IntField(rect, new GUIContent("I"), parameter.GetAs<int>());
            parameter.SetAs(result);

            EditorGUIUtility.labelWidth = oldWidth;
        }

        private static void DrawAsFloat(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = width * 0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width * 0.5f;
                rect.x += rect.width;
            }

            var oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 13;

            var result = EditorGUI.FloatField(rect, new GUIContent("F"), parameter.GetAs<float>());
            parameter.SetAs(result);

            EditorGUIUtility.labelWidth = oldWidth;
        }

        private static void DrawAsBool(Rect rect, GenericParameter parameter, bool label)
        {
            if (label)
            {
                var width = rect.width;
                rect.width = rect.width * 0.45f;

                parameter.Name = EditorGUI.TextField(rect, parameter.Name, SpaceEditorStyles.EditableLabel);

                rect.width = width * 0.5f;
                rect.x += rect.width + 2;
            }

            rect.width *= 0.5f;
            rect.y -= 1;
            var result = EditorGUI.Toggle(rect, GUIContent.none, parameter.GetAs<bool>());
            parameter.SetAs(result);

            rect.y += 1;
            rect.x += 20;

            GUI.Label(rect, result.ToString(), EditorStyles.label);
        }

        #endregion

        #region Layout

        private static void LayoutAsDerivedTypePicker(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }
                
                var type = parameter.GetAs<DerivedType>();
                string content = "None";

                if (type.BaseType != null && type.BaseType.Type != null)
                {
                    if (type.TypeValue != null && type.TypeValue.Type != null)
                    {
                        content = $"{type.TypeValue.Type.Name} ({type.BaseType.Type.Name})";
                    }
                    else
                    {
                        content = "None ({type.BaseType.Type.Name})";
                    }
                }

                if (EditorGUILayout.DropdownButton(new GUIContent(content), FocusType.Keyboard))
                {
                    SerializedType sysType = type.BaseType ?? new SerializedType(parameter.HoldType.Metadata);

                    ReferenceTypePicker.ShowWindow(sysType.Type,
                        BuildDerivedTypeCallback(parameter, sysType.Type),
                        t => t.IsSubclassOf(sysType.Type));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsTypePicker(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                var type = parameter.GetAs<SerializedType>();
                string content = type.Type != null ? $"{type.Type.Name} (System.Type)" : "None (System.Type)";

                if (EditorGUILayout.DropdownButton(new GUIContent(content), FocusType.Keyboard))
                {
                    KnownTypeUtils.ShowAddParameterMenu(BuildSerializedTypeCallback(parameter));
                }                
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsObject<T>(GenericParameter parameter, bool label) where T : UnityEngine.Object
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                T result = (T) EditorGUILayout.ObjectField(parameter.GetAs<T>(), parameter.HoldType.Type, true, GUILayout.Width(FieldWidth));
                parameter.SetAs(result);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsGenericObject(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                var result = EditorGUILayout.ObjectField(parameter.GetAs<Object>(), parameter.HoldType.Type, true, GUILayout.Width(FieldWidth));
                parameter.SetAs(result);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsVec3(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                var result = EditorGUILayout.Vector3Field(GUIContent.none, parameter.GetAs<Vector3>(), GUILayout.Width(FieldWidth));
                parameter.SetAs(result);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsVec2(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                var result = EditorGUILayout.Vector2Field(GUIContent.none, parameter.GetAs<Vector2>(), GUILayout.Width(FieldWidth));
                parameter.SetAs(result);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsInt(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                var result = EditorGUILayout.IntField(parameter.GetAs<int>(), GUILayout.Width(FieldWidth));
                parameter.SetAs(result);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsFloat(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                var result = EditorGUILayout.FloatField(parameter.GetAs<float>(), GUILayout.Width(FieldWidth));
                parameter.SetAs(result);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsBool(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                var result = EditorGUILayout.Toggle(parameter.GetAs<bool>(), GUILayout.Width(FieldWidth));
                parameter.SetAs(result);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsString(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                var result = EditorGUILayout.TextField(parameter.GetAs<string>(), GUILayout.Width(FieldWidth));
                parameter.SetAs(result);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsColor(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                var result = EditorGUILayout.ColorField(parameter.GetAs<Color>(), GUILayout.Width(FieldWidth));
                parameter.SetAs(result);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void LayoutAsCurve(GenericParameter parameter, bool label)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (label)
                {
                    parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                }

                var result = EditorGUILayout.CurveField(parameter.GetAs<AnimationCurve>(), GUILayout.Width(FieldWidth));
                parameter.SetAs(result);
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }
}
