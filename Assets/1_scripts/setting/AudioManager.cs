using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource audioSource;
    public AudioSource bgSource;

    public List<AudioClipData> AllClips;

    public AudioClip[] musics;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        if (musics.Length > 0)
        {
            bgSource.clip = musics[UnityEngine.Random.Range(0, musics.Length)];
            bgSource.loop = true;
            bgSource.Play();
        }
    }

    public void OnButtonClickSound()
    {
        audioSource.PlayOneShot(AllClips.Find(data => data.AudioClipName == AudioClipNames.ButtonClick).Clip);
    }


    public void PlayOneShot(AudioClipNames audioClipName)
    {
        audioSource.PlayOneShot(AllClips.Find(data => data.AudioClipName == audioClipName).Clip);
    }

    public void ApplyAudioSettings(bool soundOn, bool musicOn)
    {
        audioSource.mute = !soundOn;
        bgSource.mute = !musicOn;
    }
}

[Serializable]
public class AudioClipData
{
    public AudioClipNames AudioClipName;
    public AudioClip Clip;
}

public enum AudioClipNames
{
    ButtonClick,
    ButtonFail,
    BallBounce,
    BallMerge,
    DestroyRing,
}

