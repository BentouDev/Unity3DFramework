using UnityEngine.Rendering.PostProcessing;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Rendering.PostProcessing
{
    [PostProcessEditor(typeof(ColorShift))]
    public class ColorShiftEditor : PostProcessEffectEditor<ColorShift>
    {
        SerializedParameterOverride RedShift;
        SerializedParameterOverride GreenShift;
        SerializedParameterOverride BlueShift;

        public override void OnEnable()
        {
            RedShift   = FindParameterOverride(x => x.RedShift);
            GreenShift = FindParameterOverride(x => x.GreenShift);
            BlueShift  = FindParameterOverride(x => x.BlueShift);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(RedShift);
            PropertyField(GreenShift);
            PropertyField(BlueShift);
        }
    }
}