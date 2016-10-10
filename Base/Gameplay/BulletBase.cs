using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public abstract class BulletBase : MonoBehaviour
{
    public int Damage;

    public Vector3 Direction { get; set; }

    public float Speed { get; set; }

    public float LifeTime { get; set; }

    public GameObject OnDeath;
    public GameObject OnHit;

    public float DeathDuration;
    public float HitDuration;

    public virtual void OnUpdate()
    {

    }

    public virtual void Kill()
    {
        var obj = Instantiate(OnDeath, transform.position, Quaternion.identity) as GameObject;
        obj.GetComponent<LifeTime>().OnStart(DeathDuration);
    }

    public virtual void Hit()
    {
        var obj = Instantiate(OnHit, transform.position, Quaternion.identity) as GameObject;
        obj.GetComponent<LifeTime>().OnStart(HitDuration);
    }
}
