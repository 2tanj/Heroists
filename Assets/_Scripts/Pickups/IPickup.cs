using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class IPickup : IAudioPlayer, IHoverable
{
    [field: SerializeField]
    public string Name       { get; private set; }
    
    [field: SerializeField, TextArea]
    public string Description { get; private set; }

    [field: SerializeField] public   AudioClip      Sound { get; private set; }
           [SerializeField] private  SpriteRenderer _spritePrefab;

    public void Spawn(Node node)
    {
        var item = Instantiate(_spritePrefab, node.transform.position, Quaternion.identity, node.transform);
        item.gameObject.AddComponent<IPickup>(); // prints warning but works

        node.Pickup = item.GetComponent<IPickup>();
    }

    public abstract void UsePickup(Unit unit);

    public void ShowInfo() => GUIManager.Instance.ShowInfoGUI(
        _spritePrefab.sprite, Name, Description);
}
