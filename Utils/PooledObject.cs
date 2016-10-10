using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private PrefabPool pool;

    public void SetPool(PrefabPool pool)
    {
        this.pool = pool;
    }

    public void ReturnToPool()
    {
        pool.ReturnObject(gameObject);
    }
}
