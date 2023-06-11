using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class IAudioPlayer : MonoBehaviour // interface cant have attributes
{
    internal AudioSource _audio;

    [SerializeField, Header("Audio")]
    private float _pitchRandomness = .05f;
    private float _basePitch;

    internal void SetupAudio()
    {
        _audio = GetComponent<AudioSource>();
        _basePitch = _audio.pitch;
    }

    public void PlaySound(AudioClip sound)
    {
        if (!_audio)
        {
            Debug.LogWarning("You need to call SetupAudio() in Awake()! (Ignore if this was printed on game start)");
            return;
        }
        else if (!sound)
        {
            Debug.LogWarning("The sound youre trying to play is null!");
            return;
        }

        //_audio.Stop();
        _audio.clip = sound;
        _audio.Play();
    }
    public void PlaySoundWithVariablePitch(AudioClip sound) 
    {
        var randomPitch = Random.Range(-_pitchRandomness, _pitchRandomness);
        _audio.pitch = _basePitch + randomPitch;

        PlaySound(sound);
    }
}
