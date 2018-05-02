using UnityEngine.Rendering.PostProcessing;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Rendering.PostProcessing
{
    [PostProcessEditor(typeof(Glitch))]
    public class GlitchEditor : PostProcessEffectEditor<Glitch>
    {
        SerializedParameterOverride Displacement;
        SerializedParameterOverride Strength;
        SerializedParameterOverride ColorBleed;
        SerializedParameterOverride FlipIntensity;
        SerializedParameterOverride NoiseDistance;
        
        public override void OnEnable()
        {
            Displacement  = FindParameterOverride(x => x.Displacement);
            Strength      = FindParameterOverride(x => x.Strength);
            ColorBleed    = FindParameterOverride(x => x.ColorBleed);
            FlipIntensity = FindParameterOverride(x => x.FlipIntensity);
            NoiseDistance = FindParameterOverride(x => x.NoiseDistance);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(Displacement);

            if (Displacement.value.objectReferenceValue)
            {
                PropertyField(Strength);
                PropertyField(ColorBleed);
                PropertyField(FlipIntensity);
                PropertyField(NoiseDistance);
            }
        }
    }
}