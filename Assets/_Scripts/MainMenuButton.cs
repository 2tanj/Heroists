using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// small script for playing pointer enter and exit sounds 
// for better game feel
public class MainMenuButton : IAudioPlayer, IPointerEnterHandler, IPointerExitHandler
{
    [field: SerializeField] public AudioClip Sound { get; internal set; }

    public Button ButtonRef { get; internal set; }
    
    private void Awake() => SetupAudio();

    public void OnPointerEnter(PointerEventData eventData) => PlaySound(Sound);
    public void OnPointerExit(PointerEventData eventData)  => PlaySound(Sound);
}
