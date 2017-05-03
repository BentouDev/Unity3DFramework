using System;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace Framework
{
    [Serializable]
    public class GradientFogModel : PostProcessingModel
    {
        [Serializable]
        public struct Settings
        {
            [Tooltip("Should the fog affect the skybox?")]
            public bool excludeSkybox;

            public Color TopColor;

            public Color MidColor;

            public Color BottomColor;

            [Range(0.001f, 0.999f)]
            public float Blend;

            [Range(0.001f, 0.999f)]
            public float Minimum;

            [Range(0.001f, 0.999f)]
            public float Maximum;

            public static Settings defaultSettings
            {
                get
                {
                    return new Settings
                    {
                        excludeSkybox = true,
                        TopColor = Color.blue,
                        MidColor = Color.white,
                        BottomColor = Color.gray,
                        Blend = 0.5f,
                        Minimum = 0.35f,
                        Maximum = 0.65f
                    };
                }
            }
        }

        [SerializeField]
        Settings m_Settings = Settings.defaultSettings;
        public Settings settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }

        public override void Reset()
        {
            m_Settings = Settings.defaultSettings;
        }
    }
}