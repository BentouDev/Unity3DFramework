using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(ColorShiftRenderer), PostProcessEvent.AfterStack, "Framework/ColorShift")]
    public class ColorShift : PostProcessEffectSettings
    {
        public Vector2Parameter RedShift   = new Vector2Parameter();
        public Vector2Parameter GreenShift = new Vector2Parameter();
        public Vector2Parameter BlueShift  = new Vector2Parameter();
    }
}