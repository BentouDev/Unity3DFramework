using UnityEngine.Rendering.PostProcessing;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Rendering.PostProcessing
{
    [PostProcessEditor(typeof(DitheringModel))]
    public class DitheringModelEditor : PostProcessEffectEditor<DitheringModel>
    {
        SerializedParameterOverride Pattern;
        SerializedParameterOverride DitherSize;
        
        SerializedParameterOverride OriginalContribution;
        SerializedParameterOverride DitherContribution;
        SerializedParameterOverride NominalValue;
        
        SerializedParameterOverride TextureScale;

        public override void OnEnable()
        {
            Pattern = FindParameterOverride(x => x.Pattern);
            DitherSize = FindParameterOverride(x => x.DitherSize);
            
            OriginalContribution = FindParameterOverride(x => x.OriginalContribution);
            DitherContribution = FindParameterOverride(x => x.DitherContribution);
            NominalValue = FindParameterOverride(x => x.NominalValue);
            
            TextureScale = FindParameterOverride(x => x.TextureScale);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(Pattern);

            if (Pattern.value.objectReferenceValue)
            {
                PropertyField(TextureScale);

                PropertyField(DitherSize);
            
                PropertyField(OriginalContribution);
                PropertyField(DitherContribution);
                PropertyField(NominalValue);
            }
        }
    }
}