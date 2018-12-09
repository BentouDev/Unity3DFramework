using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Damageable : MonoBehaviour
{
    public bool InitOnStart;
    
    public bool Invincible;
    public int  MaxHealth;
    public int  StartingHealth;
    public float Delay = 1;

    private float LastHurt;

    public int  CurrentHealth { get; private set; }

    public bool IsAlive { get { return CurrentHealth > 0; } }

    public Animator Anim;
    public string OnHit;
    public string OnDed;
    public AnimationPlayer DeathAnim;

    void Start()
    {
        if (!InitOnStart)
            return;

        Init();
    }

    public void Init()
    {
        CurrentHealth = StartingHealth;
        Anim = GetComponentInChildren<Animator>();
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        CurrentHealth  = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
    }

    public void Hurt(int amount)
    {
        if (Invincible || !IsAlive)
            return;

        if (Time.time - LastHurt < Delay)
            return;
        
        if (Anim)
            Anim.SetTrigger(OnHit);
        
        LastHurt = Time.time;
        CurrentHealth -= amount;
        CurrentHealth  = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        
        if (!IsAlive)
            Dead();
    }

    public void Dead()
    {
        if (Anim)
            Anim.SetBool(OnDed, true);
        
        DeathAnim.Play();
    }

    void Update()
    {
        if (Anim)
        {
            Anim.SetBool(OnDed, !IsAlive);
        }
        
        DeathAnim.Update();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isEditor)
            return;
        
        StartingHealth = Mathf.Clamp(StartingHealth, 0, MaxHealth);
    }

    void OnDrawGizmosSelected()
    {
        Handles.Label(transform.position + Vector3.up * 1, "HP : " + CurrentHealth + "/" + MaxHealth);
    }
#endif
}
