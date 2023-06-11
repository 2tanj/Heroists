using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityGUIHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _image;

    private IAbility? _ability;
    public  IAbility Ability { 
        get => _ability; 
        set {
            if (!value) return;

            _ability = value;
            _image.sprite = _ability.Icon.sprite;
        } 
    }

    private void Awake() => _image = GetComponent<Image>();

    public void OnPointerExit(PointerEventData eventData)
    {
        GUIManager.Instance.HideInfoGUI();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Ability) return;
        Ability.ShowInfo();
    }
}
