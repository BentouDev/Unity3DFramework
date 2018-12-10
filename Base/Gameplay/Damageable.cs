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
    public UnityEvent OnDeath;
    public UnityEvent OnGotHit;
    public AnimationPlayer DeathAnim;

    private bool PlayedDead;

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
        
        LastHurt = Time.time;
        CurrentHealth -= amount;
        CurrentHealth  = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        if (!IsAlive)
        {
            Dead();
        }
        else
        {
            if (Anim&& !string.IsNullOrEmpty(OnHit))
                Anim.SetTrigger(OnHit);    
        }

        OnGotHit.Invoke();
    }

    public void Dead()
    {
        PlayedDead = true;

        if (Anim && !string.IsNullOrEmpty(OnDed))
            Anim.SetTrigger(OnDed);

        DeathAnim.Play();
        OnDeath.Invoke();
    }

    void Update()
    {
        if (!PlayedDead && !IsAlive)
        {
            Dead();
        }
        else
        {
            DeathAnim.Update();
        }
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
