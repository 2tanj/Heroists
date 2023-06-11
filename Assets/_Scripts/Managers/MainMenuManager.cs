using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : IAudioPlayer
{
    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private Transform _logoParent    ;
    [SerializeField] private Transform _notification  ;
    [SerializeField] private Transform _settingsParent;
    [SerializeField] private Transform _tutorialParent;
    [SerializeField] private Slider    _volumeSlider  ;
                     private bool      _areSettingsPressed = false;
                     private bool      _isNotiPressed      = false;
                     private bool      _isExitPresed       = false;
                     private bool      _isTutorialOut      = false;

    [SerializeField] private IAudioPlayer _musicPlayer;

    void Awake()
    {
        SetupAudio();
        _volumeSlider.value = _musicPlayer._audio.volume;
        Cursor.SetCursor(_cursorTexture, Vector2.zero, CursorMode.Auto);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            AnimateSettings();
    }



    //TODO: block mouse click while coroutine is running
    public void AnimateNotification()   => StartCoroutine(MoveNotifications());
    public void AnimateTutorialPrompt() => AnimateGUIOffscreen(_tutorialParent.transform, ref _isTutorialOut, 500);
    public void AnimateSettings()       => AnimateGUIOffscreen(_settingsParent.transform, ref _areSettingsPressed, -1468);
    private IEnumerator MoveNotifications()
    {
        AnimateGUIOffscreen(_notification, ref _isNotiPressed, 500);
        yield return new WaitForSeconds(1.5f);
        AnimateGUIOffscreen(_notification, ref _isNotiPressed, 500);
    }

    public static void AnimateGUIOffscreen(Transform toAnimate, ref bool flag, float distance) 
    {
        var newPos = flag ? 
            toAnimate.transform.position - new Vector3(distance, 0) : 
            toAnimate.transform.position + new Vector3(distance, 0);
        toAnimate.transform.DOMove(newPos, .5f);
        
        flag = !flag;
    }

    public void PlayGame    () => SceneManager.LoadScene(2);
    public void PlayTutorial() => SceneManager.LoadScene(1);
    public void ChangeVolume(Slider s) => _musicPlayer._audio.volume = s.value;

    public void CloseGame(Button b) 
    {
        if (_isExitPresed && EventSystem.current.currentSelectedGameObject == b.gameObject)
        {
            Debug.LogWarning("Exiting.");
            Application.Quit();
        }

        StartCoroutine(DeselectExit(b));
    }

    private IEnumerator DeselectExit(Button b)
    {
        _isExitPresed = true;
        yield return new WaitForSeconds(1.5f);

        _isExitPresed = false;
        if (EventSystem.current.currentSelectedGameObject == b.gameObject)
            EventSystem.current.SetSelectedGameObject(null);
    }
    
    private void AnimateLogo() => 
        _logoParent.DOMoveY(_logoParent.transform.position.y - 35, 1f)
                   .SetLoops(-1, LoopType.Yoyo)
                   .SetEase(Ease.Linear);
}
