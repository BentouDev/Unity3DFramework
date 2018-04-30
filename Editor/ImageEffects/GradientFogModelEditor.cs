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
        SerializedParameterOverride  m_GradientSize;

        public override void OnEnable()
        {
            m_ExcludeSkybox = FindParameterOverride(x => x.ExcludeSkybox);
            m_TopColor = FindParameterOverride(x => x.TopColor);
            m_MidColor = FindParameterOverride(x => x.MidColor);
            m_BottomColor = FindParameterOverride(x => x.BottomColor);
            m_Middle = FindParameterOverride(x => x.Blend);
            m_GradientSize = FindParameterOverride(x => x.GradientSize);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This effect adds fog compatibility to the deferred rendering path; Allows to override fog color with gradient; Rest of fog settings should be set in the Lighting panel.", MessageType.Info);
            PropertyField(m_ExcludeSkybox);
            PropertyField(m_TopColor);
            PropertyField(m_MidColor);
            PropertyField(m_BottomColor);
            PropertyField(m_Middle);
            PropertyField(m_GradientSize);
        }
    }
}
