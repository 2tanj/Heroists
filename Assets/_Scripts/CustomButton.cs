using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomButton : IAudioPlayer
{
    [field: SerializeField] public AudioClip ClickSound { get; internal set; }
    [field: SerializeField] public AudioClip HoverSound { get; internal set; }

    public Button ButtonRef { get; internal set; }

    public void PlayButtonSound() => PlaySound(ClickSound);
}
