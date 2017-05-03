using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Default")]
    [SerializeField]
    public SettingsData Settings;

    [Header("Audio")]
    public AudioMixer MasterMixer;
    public AudioSource SoundChangeClip;
    public string MusicVolume;
    public string SoundVolume;
    
    [Header("GUI")]
    public Slider MusicSlider;
    public Slider SoundSlider;
    public Toggle AnisotropicToggle;
    public Toggle VsyncToggle;

    [System.Serializable]
    public struct EffectInfo
    {
        [SerializeField]
        public bool Enable;

        [SerializeField]
        public string Name;

        [SerializeField]
        public MonoBehaviour Effect;
    }

    [System.Serializable]
    public struct SettingsData
    {
        [SerializeField]
        public float MusicVolume;

        [SerializeField]
        public float SoundVolume;

        public List<EffectInfo> Effects;
    }

    private void Start()
    {
        if (MusicSlider)
        {
            var volume = PlayerPrefs.GetFloat("MusicVolume", Settings.MusicVolume);
            MusicSlider.value = DecibelToLinear(volume);
            MasterMixer.SetFloat(MusicVolume, volume);
            MusicSlider.onValueChanged.AddListener(SetMusic);
        }

        if (SoundSlider)
        {
            var volume = PlayerPrefs.GetFloat("SoundVolume", Settings.SoundVolume);
            SoundSlider.value = DecibelToLinear(volume);
            MasterMixer.SetFloat(SoundVolume, volume);
            SoundSlider.onValueChanged.AddListener(SetSound);
        }

        if (VsyncToggle)
        {
            bool Vsync = PlayerPrefs.GetInt("Vsync", QualitySettings.vSyncCount) == 1;
            VsyncToggle.isOn = Vsync;
            QualitySettings.vSyncCount = Vsync ? 1 : 0;
            VsyncToggle.onValueChanged.AddListener(SetVSync);
        }

        if (AnisotropicToggle)
        {
            bool aniso = PlayerPrefs.GetInt("Anisotropic", (int)QualitySettings.anisotropicFiltering) == (int)AnisotropicFiltering.ForceEnable;
            AnisotropicToggle.isOn = aniso;
            QualitySettings.anisotropicFiltering = aniso ? AnisotropicFiltering.ForceEnable : AnisotropicFiltering.Disable;
            AnisotropicToggle.onValueChanged.AddListener(SetAnisotropic);
        }
    }

    public void SetMusic(float volume)
    {
        var dB = LinearToDecibel(volume);
        MasterMixer.SetFloat(MusicVolume, dB);
        PlayerPrefs.SetFloat("MusicVolume", dB);
    }

    public void SetSound(float volume)
    {
        var dB = LinearToDecibel(volume);
        MasterMixer.SetFloat(SoundVolume, dB);
        PlayerPrefs.SetFloat("SoundVolume", dB);

        if (SoundChangeClip)
            SoundChangeClip.Play();
    }

    public void SetAliasing(float level)
    {
        QualitySettings.antiAliasing = level == 0 ? 0 : (int)Mathf.Pow(2, level);
    }

    public void SetAnisotropic(bool value)
    {
        QualitySettings.anisotropicFiltering = value ? AnisotropicFiltering.ForceEnable : AnisotropicFiltering.Disable;
        PlayerPrefs.SetInt("Anisotropic", (int) QualitySettings.anisotropicFiltering);
    }

    public void SetVSync(bool value)
    {
        QualitySettings.vSyncCount = value ? 1 : 0;
        PlayerPrefs.SetInt("Vsync", QualitySettings.vSyncCount);
    }

    public static float LinearToDecibel(float linear)
    {
        float dB;

        if (linear != 0)
            dB = 20.0f * Mathf.Log10(linear);
        else
            dB = -144.0f;

        return dB;
    }

    public static float DecibelToLinear(float dB)
    {
        float linear = Mathf.Pow(10.0f, dB / 20.0f);

        return linear;
    }
}
