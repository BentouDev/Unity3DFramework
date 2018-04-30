using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(GradientFogRenderer), PostProcessEvent.AfterStack, "Unity/Gradient Fog")]
    public class GradientFogModel : PostProcessEffectSettings
    {
        [Tooltip("Should the fog affect the skybox?")]
        public BoolParameter ExcludeSkybox = new BoolParameter { value = true };

        [Tooltip("Top Fog Color")]
        public ColorParameter TopColor = new ColorParameter { value = Color.blue};

        [Tooltip("Middle Fog Color")]
        public ColorParameter MidColor = new ColorParameter { value = Color.white};

        [Tooltip("Bottom Fog Color")]
        public ColorParameter BottomColor = new ColorParameter { value = Color.gray};

        [Tooltip("Blend factor")]
        [Range(0.001f, 0.999f)]
        public FloatParameter Blend = new FloatParameter { value = 0.5f };

        [MinMax(0.001f, 0.999f), DisplayName("Gradient Size")]
        public Vector2Parameter GradientSize = new Vector2Parameter { value = new Vector2(0.35f, 0.65f) };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value
                && RenderSettings.fog
                && !RuntimeUtilities.scriptableRenderPipelineActive
                //&& context.resources.shaders.deferredFog
                //&& context.resources.shaders.deferredFog.isSupported
                && context.camera.actualRenderingPath == RenderingPath.DeferredShading;  // In forward fog is already done at shader level
        }
    }
}