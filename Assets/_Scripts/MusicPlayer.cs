using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : IAudioPlayer
{
    // TODO: make it a queue and cycle songs
    [SerializeField] private AudioClip _music;

    void Awake()
    {
        SetupAudio();
        _audio.loop = true;
    }
    void Start() => PlaySound(_music);


}
