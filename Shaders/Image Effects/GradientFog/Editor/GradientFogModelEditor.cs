using UnityEngine.PostProcessing;
using UnityEditor.PostProcessing;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    using Settings = GradientFogModel.Settings;

    [PostProcessingModelEditor(typeof(GradientFogModel))]
    public class GradientFogModelEditor : PostProcessingModelEditor
    {
        SerializedProperty m_ExcludeSkybox;
        SerializedProperty m_TopColor;
        SerializedProperty m_MidColor;
        SerializedProperty m_BottomColor;
        SerializedProperty m_Middle;
        SerializedProperty m_Min;
        SerializedProperty m_Max;

        public override void OnEnable()
        {
            m_ExcludeSkybox = FindSetting((Settings x) => x.excludeSkybox);
            m_TopColor = FindSetting((Settings x) => x.TopColor);
            m_MidColor = FindSetting((Settings x) => x.MidColor);
            m_BottomColor = FindSetting((Settings x) => x.BottomColor);
            m_Middle = FindSetting((Settings x) => x.Middle);
            m_Min = FindSetting((Settings x) => x.Minimum);
            m_Max = FindSetting((Settings x) => x.Maximum);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This effect adds fog compatibility to the deferred rendering path; Allows to override fog color with gradient; Rest of fog settings should be set in the Lighting panel.", MessageType.Info);
            EditorGUILayout.PropertyField(m_ExcludeSkybox, EditorGUIHelper.GetContent("Exclude Skybox (deferred only)"));
            EditorGUILayout.PropertyField(m_TopColor, EditorGUIHelper.GetContent("Top Fog Color (deferred only)"));
            EditorGUILayout.PropertyField(m_MidColor, EditorGUIHelper.GetContent("Mid Fog Color (deferred only)"));
            EditorGUILayout.PropertyField(m_BottomColor, EditorGUIHelper.GetContent("Bottom Fog Color (deferred only)"));
            EditorGUILayout.PropertyField(m_Middle, EditorGUIHelper.GetContent("Middle value (deferred only)"));

            float min = m_Min.floatValue;
            float max = m_Max.floatValue;

            EditorGUILayout.MinMaxSlider (
                EditorGUIHelper.GetContent("Middle section | Values between which middle section is placed."),
                ref min, ref max, 0.001f, 0.999f
            );

            m_Min.floatValue = Mathf.Min(min, max - 0.001f);
            m_Max.floatValue = Mathf.Max(max, min + 0.001f);
        }
    }
}
