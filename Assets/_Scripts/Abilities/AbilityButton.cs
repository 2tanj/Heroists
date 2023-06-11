using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityButton : CustomButton, IPointerEnterHandler, IPointerExitHandler
{
    [field: SerializeField]
    public IAbility     Ability { get; set; }

    [SerializeField]
    private AbilityType _type;

    private void Awake()
    {
        SetupAudio();

        ButtonRef = GetComponent<Button>();
        ButtonRef.onClick.AddListener(() => {
            PlaySound(ClickSound);
            GameManager.Instance.SetAbilityType(_type, this);
            });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Ability == null) return;

        Ability.ShowInfo();
        PlaySound(HoverSound);
    }

    public void OnPointerExit(PointerEventData eventData) => 
        GUIManager.Instance.HideInfoGUI();
}
