using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class AudioRandomizer : MonoBehaviour
{
    [Range(-3,3)]
    public float MinPitch = 1;

    [Range(-3, 3)]
    public float MaxPitch = 1;

    public List<AudioSource> Sources = new List<AudioSource>();

    private AudioSource lastSource;
    
    public void Play()
    {
        if (Sources.Count == 0)
            return;

        var index = Mathf.RoundToInt(Random.Range(0, Sources.Count));
        var audio = Sources[index];

        Randomize(audio);
        audio.Play();

        lastSource = audio;
    }

    public void Randomize(AudioSource source)
    {
        source.pitch = Random.Range(MinPitch, MaxPitch);
    }

    public void LoadForCharacter(List<AudioClip> clips, AudioMixerGroup group, float minPitch, float maxPitch)
    {
        Sources.Clear();
        foreach (AudioClip clip in clips)
        {
            var go = new GameObject(clip.name);
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;

            var source = go.AddComponent<AudioSource>();
                source.clip = clip;
                source.outputAudioMixerGroup = group;

            Sources.Add(source);
        }

        MinPitch = minPitch;
        MaxPitch = maxPitch;
    }

    public void StopLoop()
    {
        if (lastSource)
            lastSource.loop = false;
    }
}
