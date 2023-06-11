using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private Transform       _tutorialPrompt;
    [SerializeField] private TextMeshProUGUI _tutorialText;

    [SerializeField] private Transform _selectedHeroPanel;
    [SerializeField] private Transform _infoPanel;
    [SerializeField] private Transform _abilitiesPanel;
    [SerializeField] private Transform _heroesPanel;
    [SerializeField] private Transform _coinsParent;

    private bool _spacePressed = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            _spacePressed = true;
    }

    private void Awake()
    {
        StartCoroutine(StartTutorial());
    }

    private IEnumerator StartTutorial()
    {
        yield return StartCoroutine(TutorialPrompt("Welcome to heroists. Press SPACE to proceed"));
        yield return StartCoroutine(TutorialPrompt("You can pan the camera by pressing MIDDLE MOUSE BUTTON."));
        yield return StartCoroutine(TutorialPrompt("You can zoom using the SCROLL WHEEL"));
        yield return StartCoroutine(TutorialPrompt("In the bottom left of the screen is the selected unit panel.", () => _selectedHeroPanel.gameObject.SetActive(true))); // select a hero for panel to show up
        yield return StartCoroutine(TutorialPrompt("It shows you everything you need to know about the selected unit."));
        yield return StartCoroutine(TutorialPrompt("The icons show the units health, strength and defense capabilites."));
        yield return StartCoroutine(TutorialPrompt("You can find the units abilities next to their avatar(if they have them)")); 
        yield return StartCoroutine(TutorialPrompt("Hovering your mouse over them shows you a detailed description", () => _infoPanel.gameObject.SetActive(true))); // show ability info
        yield return StartCoroutine(TutorialPrompt("Upon hovering any object a new info panel will apear at the top of the screen (ex: hero on field)"));         yield return StartCoroutine(TutorialPrompt("You can see your current hero's available abilites at the bottom(you can hover them for information)", () => _abilitiesPanel.gameObject.SetActive(true))); // show ability panel
        yield return StartCoroutine(TutorialPrompt("If it's your turn you can select an ability by pressing the button or its keybind", () => GUIManager.Instance.SetupAbilityButtons((HeroUnit)GameManager.Instance.FriendlyUnits[0])));
        yield return StartCoroutine(TutorialPrompt("Upon choosing an ability you will be able to see the range and valid moves of the selected ability"));
        yield return StartCoroutine(TutorialPrompt("Green means that you can use the ability on square and blue shows the range")); 
        yield return StartCoroutine(TutorialPrompt("You can also hover any enemy and see their attack range(red) and the path they will take on the next move(orange)", () => { // spawn enemy and focus camera on it
            GameManager.Instance.EnemyUnits[0].gameObject.SetActive(true);
            CameraController.FollowUnit(GameManager.Instance.EnemyUnits[0]);
            CameraController.FollowUnit(null);
        })); 
        yield return StartCoroutine(TutorialPrompt("In the bottom right corner of your screen lies the hero selection panel", () => {
            _heroesPanel.gameObject.SetActive(true); // show heroes panel and coins
            _coinsParent.gameObject.SetActive(true);
        }));
        yield return StartCoroutine(TutorialPrompt("Here you can see all of the heroes you have unlocked"));
        yield return StartCoroutine(TutorialPrompt("If you have enough gold you can purchase a hero by selecting it and placing it on a wanted node."));
        yield return StartCoroutine(TutorialPrompt("That's it for now. You can play around or jump straight into action by pressing SPACE", () => {
            GameManager.Instance.IsTutorialScene = false;
            SceneManager.LoadScene(2);
        }));
    }

    private IEnumerator TutorialPrompt(string message, Action action = null)
    {
        _tutorialText.text = message;
        if (action != null)
            action.Invoke();

        yield return new WaitUntil(() => _spacePressed);
        _spacePressed = false;
    }

    private IEnumerator Controls(Action ac)
    {
        Debug.Log("These are the controls!");
        yield return new WaitUntil(() => _spacePressed);
        _spacePressed = false;
    }

}