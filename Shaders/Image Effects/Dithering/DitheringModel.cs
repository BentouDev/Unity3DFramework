using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(DitheringRenderer), PostProcessEvent.AfterStack, "Framework/Dithering")]
    public class DitheringModel : PostProcessEffectSettings
    {
        public TextureParameter Pattern = new TextureParameter();
        
        public IntParameter DitherSize = new IntParameter { value = 4 };

        public FloatParameter OriginalContribution = new FloatParameter { value = 128 };
        public FloatParameter DitherContribution = new FloatParameter { value = 128 };
        public FloatParameter NominalValue = new FloatParameter { value = 128 };
        
        public Vector2Parameter TextureScale = new Vector2Parameter { value = new Vector2(1,1) };
    }
}