using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    public void Init()
    {
        OnInit();
    }

    public void Tick()
    {
        OnProcessControll();
    }

    protected virtual void OnInit()
    { }
    
    protected virtual void OnProcessControll()
    { }
}
