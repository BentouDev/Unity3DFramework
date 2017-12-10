using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Lifetime : MonoBehaviour
{
    public bool BeginOnStart;
    public bool DestroyOnEnd;
    public float Duration;

    public float Elapsed => Time.time - StartTime;

    private float StartTime;

    public UnityEvent OnEnd;

    void Start()
    {
        if (!BeginOnStart)
            return;

        Begin();
    }

    public void Begin()
    {
        StartTime = Time.time;
        StartCoroutine(CoLifetime());
    }

    public void Die()
    {
        StopAllCoroutines();
        StartCoroutine(CoDie());
    }

    IEnumerator CoDie()
    {
        OnEnd.Invoke();

        if (DestroyOnEnd)
        {
            yield return new WaitForFixedUpdate();
            DestroyObject(gameObject);
        }
    }

    IEnumerator CoLifetime()
    {
        yield return new WaitForSeconds(Duration);

        StartCoroutine(CoDie());
    }
}
