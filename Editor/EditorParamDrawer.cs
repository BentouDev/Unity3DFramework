using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.EditorUtils
{
    public static class EditorParamDrawer
    {
        public static int LabelWidth = 100;
        public static int FieldWidth = 150;

        public static void DrawParameter(EditorParameter parameter)
        {
            switch (parameter.HoldType)
            {
                case EditorParameter.ParameterType.Bool:
                    DrawAsBool(parameter);
                    break;
                case EditorParameter.ParameterType.Float:
                    DrawAsFloat(parameter);
                    break;
                case EditorParameter.ParameterType.Int:
                    DrawAsInt(parameter);
                    break;
                case EditorParameter.ParameterType.Object:
                    DrawAsObject(parameter);
                    break;
                case EditorParameter.ParameterType.Vec2:
                    DrawAsVec2(parameter);
                    break;
                case EditorParameter.ParameterType.Vec3:
                    DrawAsVec3(parameter);
                    break;
                default:
                    break;
            }
        }

        private static void DrawAsObject(EditorParameter parameter)
        {
            EditorGUILayout.BeginHorizontal();
            {
                parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                parameter.ObjectValue = EditorGUILayout.ObjectField(parameter.ObjectValue, parameter.Type, false, GUILayout.Width(FieldWidth));
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawAsVec3(EditorParameter parameter)
        {
            EditorGUILayout.BeginHorizontal();
            {
                parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                parameter.Vec3Value = EditorGUILayout.Vector3Field(GUIContent.none, parameter.Vec3Value, GUILayout.Width(FieldWidth));
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawAsVec2(EditorParameter parameter)
        {
            EditorGUILayout.BeginHorizontal();
            {
                parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                parameter.Vec2Value = EditorGUILayout.Vector2Field(GUIContent.none, parameter.Vec2Value, GUILayout.Width(FieldWidth));
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawAsInt(EditorParameter parameter)
        {
            EditorGUILayout.BeginHorizontal();
            {
                parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                parameter.IntValue = EditorGUILayout.IntField(parameter.IntValue, GUILayout.Width(FieldWidth));
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawAsFloat(EditorParameter parameter)
        {
            EditorGUILayout.BeginHorizontal();
            {
                parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                parameter.FloatValue = EditorGUILayout.FloatField(parameter.FloatValue, GUILayout.Width(FieldWidth));
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawAsBool(EditorParameter parameter)
        {
            EditorGUILayout.BeginHorizontal();
            {
                parameter.Name = EditorGUILayout.TextField(parameter.Name, GUILayout.Width(LabelWidth));
                parameter.BoolValue = EditorGUILayout.Toggle(parameter.BoolValue, GUILayout.Width(FieldWidth));
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
