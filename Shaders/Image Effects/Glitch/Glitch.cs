using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(GlitchRenderer), PostProcessEvent.AfterStack, "Framework/Glitch")]
    public class Glitch : PostProcessEffectSettings
    {
        public TextureParameter Displacement = new TextureParameter();
        
        [Range(0,1)]
        public FloatParameter ColorBleed = new FloatParameter { value = 1 };
        
        [Range(0,1)]
        public FloatParameter Strength = new FloatParameter { value = 1 };
        
        [Range(0, 1)]
        public FloatParameter FlipIntensity = new FloatParameter { value = 1 };
        
        public FloatParameter NoiseDistance = new FloatParameter { value = 100 };
    }
}