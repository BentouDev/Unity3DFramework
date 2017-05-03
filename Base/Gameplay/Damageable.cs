using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public bool InitOnStart;
    public int  MaxHealth;
    public int  StartingHealth;

    public int  CurrentHealth { get; private set; }

    public bool IsAlive { get { return CurrentHealth > 0; } }

    void Start()
    {
        if (!InitOnStart)
            return;

        Init();
    }

    public void Init()
    {
        CurrentHealth = StartingHealth;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isEditor)
            return;
        
        StartingHealth = Mathf.Clamp(StartingHealth, 0, MaxHealth);
    }
#endif
}
