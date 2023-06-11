using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IState : MonoBehaviour
{
    [field: SerializeField]
    public string Name { get; private set; }

    internal Unit _parent;

    public virtual void Enter(Unit u) 
    {
        _parent = u;
        Debug.Log(_parent.Stats.Name + " entered: " + Name + " state");
    }
    public virtual void Exit() 
    {
        Debug.Log(_parent.Stats.Name + " exited: " + Name + " state");
        Destroy(gameObject);
    }
    public abstract void Execute();
    public abstract void SetItem(object item);
}
