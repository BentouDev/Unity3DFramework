using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(GradientFogComponent), PostProcessEvent.BeforeStack, "Unity/Gradient Fog")]
    public class GradientFogModel : PostProcessEffectSettings
    {
        [Tooltip("Should the fog affect the skybox?")]
        public bool excludeSkybox = true;

        public ColorParameter TopColor = new ColorParameter { value = Color.blue};

        public ColorParameter MidColor = new ColorParameter { value = Color.white};

        public ColorParameter BottomColor = new ColorParameter { value = Color.gray};

        [Range(0.001f, 0.999f)]
        public FloatParameter Blend = new FloatParameter { value = 0.5f };

        [Range(0.001f, 0.999f)]
        public FloatParameter Minimum = new FloatParameter { value = 0.35f };

        [Range(0.001f, 0.999f)]
        public FloatParameter Maximum = new FloatParameter { value = 0.65f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value
                   // && context.isGBufferAvailable // In forward fog is already done at shader level
                && RenderSettings.fog;
            // && !context.interrupted;
        }
        
//        public override DepthTextureMode GetCameraFlags()
//        {
//            return DepthTextureMode.Depth;
//        }
//
//        public override CameraEvent GetCameraEvent()
//        {
//            return CameraEvent.AfterImageEffectsOpaque;
//        }
    }
}