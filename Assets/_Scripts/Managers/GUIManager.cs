using System.Collections;
using System.Collections.Generic;
using DamageNumbersPro;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance;

    #region REFERENCES
    [Header("Selected Unit")]
    [SerializeField] private Transform        _selectedPanel   ;
    [SerializeField] private TextMeshProUGUI  _unitNameText    ;
    [SerializeField] private Image            _unitIcon        ;
    [SerializeField] private AbilityGUIHelper _unitOffAbility  ;
    [SerializeField] private AbilityGUIHelper _unitDefAbility  ;
                     private Vector3          _selectedStartPos;

    [Header("Selected Unit Sliders")]
    [SerializeField] private Image          _hpCounterValue     ;
    [SerializeField] private Image          _defenseCounterValue;
    [SerializeField] private Image          _attackCounterValue ;

    [field: Header("Ability Buttons")]
    [field: SerializeField] public AbilityButton MoveButton      { get; private set; }
    [field: SerializeField] public AbilityButton AttackButton    { get; private set; }
    [field: SerializeField] public AbilityButton OffensiveButton { get; private set; } // rude button :(
    [field: SerializeField] public AbilityButton DefensiveButton { get; private set; } // nice button :)

    [SerializeField] private Image           _offensiveIcon  ;
    [SerializeField] private Image           _defensiveIcon  ;
    [SerializeField] private Image           _cooldownIcon   ;
    [SerializeField] private Image           _offensiveCdRing;
    [SerializeField] private Image           _defensiveCdRing;

    [Header("Info Panel")]
    [SerializeField] private Transform       _infoParent  ;
    [SerializeField] private Image           _infoIcon    ;
    [SerializeField] private TextMeshProUGUI _infoName    ;
    [SerializeField] private TextMeshProUGUI _infoDesc    ;
                     private Vector3         _infoStartPos;

    [Header("Hero Selection")]
    [SerializeField] private Transform       _selectionParent ;
    [SerializeField] private HeroButton      _heroButtonPrefab;

    [field: Header("Counters")]
    [field: SerializeField] public TextMeshProUGUI  CoinText { get; set; }
    [field: SerializeField] public TextMeshProUGUI  RoundText { get; set; }

    [Header("Settings")]
    [SerializeField] private Slider    _volumeSlider;
    [SerializeField] private Transform _settingsParent;
                     private bool _areSettingsPressed;
                     private bool _isVolumePressed;

    [Header("Cursors")]
    [SerializeField] private Texture2D       _cursorTexture          ;
    [SerializeField] private Texture2D       _selectableCursorTexture;

    [Header("Music player")]
    [SerializeField] private MusicPlayer     _musicPlayer;

    [Header("Damage numbers")]
    [SerializeField] private DamageNumber    _damageNumberPrefab;
    [SerializeField] private DamageNumber    _healNumberPrefab;

    [Header("Key Bindings - unused currently")]
    [SerializeField] private KeyCode         _moveBind     ;
    [SerializeField] private KeyCode         _atkBind      ;
    [SerializeField] private KeyCode         _offensiveBind;
    [SerializeField] private KeyCode         _defensiveBind;
    [SerializeField] private KeyCode         _cancelBind   ;
    #endregion

    void Awake() => Instance = this;
    void Start()
    {
        _volumeSlider.value = _musicPlayer._audio.volume;

        _infoStartPos     = _infoParent   .transform.position;
        _selectedStartPos = _selectedPanel.transform.position;

        CoinText.text  = GameManager.Instance.CoinCointer .ToString();
        RoundText.text = GameManager.Instance.RoundCounter.ToString();

        SetDefaultCursor();
        SetupHeroSelection();
    }
    // TODO: remove after done with debuging
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            ShowInfoGUI(null, "cao", "asd");
        else if (Input.GetKeyDown(KeyCode.X))
            HideInfoGUI();
    }

    // false = basic / true = selectable
    public void SetDefaultCursor(bool selectableCursor = false) 
    {
        var cursor = selectableCursor ? _selectableCursorTexture : _cursorTexture;
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
    }
    private float GetCounterValue(float current, float max) => current / max;


    public void ShowInfoGUI(Sprite icon, string itemName, string description) 
    {
        SetDefaultCursor(true);


        _infoIcon.sprite = icon;
        _infoName.text   = itemName;
        _infoDesc.text   = description;

        // dont do the animation if in tutorial (buggy)
        if (GameManager.Instance.IsTutorialScene)
            return;

        var newPos = _infoStartPos;
        newPos.y  -= (float)(Screen.height / 2.8);
        _infoParent.transform.DOMove(newPos, .12f)
            .OnComplete(() => {
                newPos.y += 10f;
                _infoParent.transform.DOMove(newPos, .08f);
            });
    }
    public void HideInfoGUI()
    {
        SetDefaultCursor();

        _infoParent.transform.DOMove(_infoStartPos, .08f);
    }

    public void ShowSelectedUnitInfo(Unit unit)
    {
        if (!unit.OffensiveAbility)
            _unitOffAbility.gameObject.SetActive(false);
        else
        {
            _unitOffAbility.Ability = unit.OffensiveAbility;
            _unitOffAbility.gameObject.SetActive(true);
        }
        
        if (!unit.DefensiveAbility)
            _unitDefAbility.gameObject.SetActive(false);
        else
        {
            _unitDefAbility.Ability = unit.DefensiveAbility;
            _unitDefAbility.gameObject.SetActive(true);
        }

        _unitNameText.text = unit.Stats.Name;
        _unitIcon.sprite   = unit.GetComponent<SpriteRenderer>().sprite;

        _unitDefAbility.Ability = unit.DefensiveAbility;

        _defenseCounterValue.fillAmount = GetCounterValue(unit.Stats.Defense,   1);
        _attackCounterValue .fillAmount = GetCounterValue(unit.Stats.AttackDamage, unit.Stats.GetMaxAtkDmg());
        _hpCounterValue     .fillAmount = GetCounterValue(unit.Stats.MaxHealth,    unit.Stats.GetMaxMaxHealth());

        // dont do the animation if in tutorial (buggy)
        if (GameManager.Instance.IsTutorialScene)
            return;

        var newPos = _selectedStartPos;
        newPos.x  += (float)(Screen.width / 4);
        _selectedPanel.transform.DOMove(newPos, .12f)
            .OnComplete(() => {
                newPos.x -= 10f;
                _selectedPanel.transform.DOMove(newPos, .08f);
            });
    }
    public void HideSelectedUnitInfo() => _selectedPanel.transform.DOMove(_selectedStartPos, .08f);

    private void SetupHeroSelection() => 
        GridManager.Instance.AllHeroes.ForEach(hero => 
        HeroButton.CreateButton(_heroButtonPrefab, _selectionParent, (HeroUnit)hero));

    public void SetupAbilityButtons(HeroUnit selected)
    {
        OffensiveButton.Ability = selected.OffensiveAbility;
        DefensiveButton.Ability = selected.DefensiveAbility;

        if (selected.OffensiveAbility.IsNotOnCooldown)
        {
            _offensiveCdRing.gameObject.SetActive(false);
            OffensiveButton.ButtonRef.interactable = true;

            _offensiveIcon.color = Color.white;
            _offensiveIcon.sprite = selected.OffensiveAbility.Icon.sprite;
        }
        else
        {
            OffensiveButton.ButtonRef.interactable = false;

            //TODO: delete the instantiated object when ability cooldown expires
            _offensiveIcon.sprite = Instantiate(_cooldownIcon, transform).sprite;
            _offensiveIcon.color = Color.red;

            _offensiveCdRing.fillAmount =
                (float)(selected.OffensiveAbility.Cooldown - 
                (GameManager.Instance.RoundCounter - selected.OffensiveAbility.UsedOnRound))
                / selected.OffensiveAbility.Cooldown;

            _offensiveCdRing.gameObject.SetActive(true);
        }

        if (selected.DefensiveAbility.IsNotOnCooldown)
        {
            DefensiveButton.ButtonRef.interactable = true;
            _defensiveCdRing.gameObject.SetActive(false);

            _defensiveIcon.color = Color.white;
            _defensiveIcon.sprite = selected.DefensiveAbility.Icon.sprite;
        }
        else
        {
            DefensiveButton.ButtonRef.interactable = false;

            //TODO: delete the instantiated object when ability cooldown expires
            _defensiveIcon.sprite = Instantiate(_cooldownIcon, transform).sprite;
            _defensiveIcon.color = Color.red;

            _defensiveCdRing.fillAmount = 
                (float)(selected.DefensiveAbility.Cooldown - 
                (GameManager.Instance.RoundCounter - selected.DefensiveAbility.UsedOnRound))
                / selected.DefensiveAbility.Cooldown;
            
            _defensiveCdRing.gameObject.SetActive(true);
        }
    }


    public void OnSettingsPressed()
    {
        AnimateSettings();
        AnimateVolumeSlider();
    }
    private void AnimateSettings()        => MainMenuManager.AnimateGUIOffscreen(_settingsParent.transform, ref _areSettingsPressed, -400);
    private void AnimateVolumeSlider()    => MainMenuManager.AnimateGUIOffscreen(_volumeSlider.transform, ref   _isVolumePressed, 90);
    public void OnVolumeChanged(Slider s) => _musicPlayer._audio.volume = s.value;

    // true = damage / false = heal
    // heal isnt used anywhere
    public void SpawnDamageNumber(Vector3 spawnPos, float number, bool damageOrHeal) 
    {
        if (damageOrHeal)
            _damageNumberPrefab.Spawn(spawnPos, number);
        else
            _healNumberPrefab.Spawn(spawnPos, number);
    }  
}
