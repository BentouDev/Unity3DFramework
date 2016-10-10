using UnityEngine;

public class LifeTime : MonoBehaviour
{
    private PrefabPool _pool;
    private float elapsed;
    private float duration;
    private bool isAlive;

    public void OnStart(float dur, PrefabPool pool = null)
    {
        isAlive = true;
        _pool = pool;
        elapsed = 0;
        duration = dur;
    }

    void Update()
    {
        if(!isAlive)
            return;
        
        elapsed += Time.deltaTime;
        if (elapsed > duration)
        {
            Kill();
        }
    }

    void Kill()
    {
        if (_pool)
        {
            _pool.ReturnObject(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}