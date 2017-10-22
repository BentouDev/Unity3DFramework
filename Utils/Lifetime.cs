using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Lifetime : MonoBehaviour
{
    public bool BeginOnStart;
    public bool DestroyOnEnd;
    public float Duration;

    public UnityEvent OnEnd;

    void Start()
    {
        if (!BeginOnStart)
            return;

        Begin();
    }

    public void Begin()
    {
        StartCoroutine(CoLifetime());
    }

    IEnumerator CoLifetime()
    {
        yield return new WaitForSeconds(Duration);

        OnEnd.Invoke();

        if (DestroyOnEnd)
        {
            yield return new WaitForFixedUpdate();
            DestroyObject(gameObject);
        }
    }
}
