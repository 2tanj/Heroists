using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class HeroButton : CustomButton, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image           _heroIcon = null;
    private TextMeshProUGUI _costText;

    [field: SerializeField]
    public  HeroUnit Hero { get; set; }
    private HeroUnit _heroRef;

    private void Awake()
    {
        SetupAudio();

        // getting the _heroIcon reference
        // cant with GetCompInChildren since this object has an image component
        //for (int i = 0; i < transform.childCount; i++)
            if (transform.GetChild(0).GetComponent<Image>() != null)
                _heroIcon = transform.GetChild(0).GetComponent<Image>();

        _heroIcon.sprite = Hero.GetComponent<SpriteRenderer>().sprite;

        _costText        = GetComponentInChildren<TextMeshProUGUI>();
        _costText.text   = Hero.Cost.ToString();

        ButtonRef = GetComponent<Button>();
        ButtonRef.onClick.AddListener(() => {
            PlaySound(ClickSound);

            var temp = Instantiate(Hero, transform);
            temp.gameObject.SetActive(false);

            GameManager.Instance.SetAbilityType(AbilityType.PLACEMENT, this);
            GameManager.Instance.HeroToPlace = temp;

            temp.ShowInfo();
        });
    }

    public static void CreateButton(HeroButton prefab, Transform parent, HeroUnit hero)
    {
        prefab.Hero = hero;
        Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        GUIManager.Instance.HideInfoGUI();
        GUIManager.Instance.HideSelectedUnitInfo();

        StartCoroutine(RemoveHeroToPlace());
    }

    // the mouse click that we use to spawn a hero-
    // deselects the button and HeroToPlace,
    // this leads to a null reference exception-
    // since deselect happens before mouse press registration/
    // so i fixed this by deselecting HeroToPlace after a little time
    // and then when we want to spawn a hero we check if HeroToPlace is null
    private IEnumerator RemoveHeroToPlace()
    {
        yield return new WaitForSeconds(.3f);
        GameManager.Instance.HeroToPlace = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GUIManager.Instance.SetDefaultCursor(true);
        PlaySound(HoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GUIManager.Instance.SetDefaultCursor(false);
    }
}
