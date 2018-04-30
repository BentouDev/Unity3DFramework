using UnityEngine.Rendering.PostProcessing;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Rendering.PostProcessing
{
    [PostProcessEditor(typeof(GradientFogModel))]
    public class GradientFogModelEditor : PostProcessEffectEditor<GradientFogModel>
    {
        SerializedParameterOverride  m_ExcludeSkybox;
        SerializedParameterOverride  m_TopColor;
        SerializedParameterOverride  m_MidColor;
        SerializedParameterOverride  m_BottomColor;
        SerializedParameterOverride  m_Middle;
        SerializedParameterOverride  m_Min;
        SerializedParameterOverride  m_Max;

        public override void OnEnable()
        {
            m_ExcludeSkybox = FindParameterOverride(x => x.excludeSkybox);
            m_TopColor = FindParameterOverride(x => x.TopColor);
            m_MidColor = FindParameterOverride(x => x.MidColor);
            m_BottomColor = FindParameterOverride(x => x.BottomColor);
            m_Middle = FindParameterOverride(x => x.Blend);
            m_Min = FindParameterOverride(x => x.Minimum);
            m_Max = FindParameterOverride(x => x.Maximum);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This effect adds fog compatibility to the deferred rendering path; Allows to override fog color with gradient; Rest of fog settings should be set in the Lighting panel.", MessageType.Info);
            PropertyField(m_ExcludeSkybox);
            PropertyField(m_TopColor);
            PropertyField(m_MidColor);
            PropertyField(m_BottomColor);
            PropertyField(m_Middle);

//            EditorGUILayout.PropertyField(m_ExcludeSkybox, new GUIContent("Exclude Skybox (deferred only)"));
//            EditorGUILayout.PropertyField(m_TopColor, new GUIContent("Top Fog Color (deferred only)"));
//            EditorGUILayout.PropertyField(m_MidColor, new GUIContent("Mid Fog Color (deferred only)"));
//            EditorGUILayout.PropertyField(m_BottomColor, new GUIContent("Bottom Fog Color (deferred only)"));
//            EditorGUILayout.PropertyField(m_Middle, new GUIContent("Blend factor (deferred only)"));

//            float min = m_Min.value;
//            float max = m_Max.value;
//
//            EditorGUILayout.MinMaxSlider (
//                new GUIContent("Gradient size | Values between which middle section is placed."),
//                ref min, ref max, 0.001f, 0.999f
//            );
//
//            m_Min.floatValue = Mathf.Min(min, max - 0.001f);
//            m_Max.floatValue = Mathf.Max(max, min + 0.001f);
        }
    }
}
