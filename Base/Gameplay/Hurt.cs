using UnityEngine;
using System.Collections;
using System.Linq;

public class Hurt : MonoBehaviour
{
    [System.Serializable]
    public struct HurtInfo
    {
        [SerializeField]
        public bool Push;

        [SerializeField]
        public Vector3 PushDirection;

        [SerializeField]
        public float PushPower;

        [SerializeField]
        public bool WhenTriggerEnter;

        [SerializeField]
        public bool WhenTriggerStay;

        [SerializeField]
        public bool WhenColliderEnter;

        [SerializeField]
        public bool WhenColliderStay;

        [SerializeField]
        public float StayHurtDelay;

        [SerializeField]
        public int Damage;

        [SerializeField]
        public Transform IgnoreParent;
    }

    public bool IsActive = true;

    [SerializeField]
    public HurtInfo Data;
    
    private float LastHurtTime;

    public void SetIgnore(Transform parent)
    {
        Data.IgnoreParent = parent;
    }
    
    public void SetActive(bool value)
    {
        IsActive = value;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsActive || !Data.WhenColliderEnter)
            return;

        DoHurt(collision.collider.gameObject, collision.contacts.OrderBy(c => (c.point - transform.position).magnitude).First().point);
    }

    void OnCollisionStay(Collision collision)
    {
        if (!IsActive || !Data.WhenColliderStay)
            return;

        if (Time.time - LastHurtTime < Data.StayHurtDelay)
            return;

        DoHurt(collision.collider.gameObject, collision.contacts.OrderBy(c => (c.point - transform.position).magnitude ).First().point);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (!IsActive || !Data.WhenTriggerEnter)
            return;

        DoHurt(collider.gameObject, collider.gameObject.transform.position);
    }

    void OnTriggerStay(Collider collider)
    {
        if (!IsActive || !Data.WhenTriggerEnter)
            return;

        if (Time.time - LastHurtTime < Data.StayHurtDelay)
            return;
        
        DoHurt(collider.gameObject, collider.gameObject.transform.position);
    }

    private void DoHurt(GameObject obj, Vector3 contact)
    {
        if (Data.IgnoreParent != null && obj.transform.IsChildOf(Data.IgnoreParent))
            return;

        var dmg = obj.GetComponentInParent(typeof(IDamageable)) as IDamageable;
        if (dmg != null)
        {
            LastHurtTime = Time.time;

            var direction = obj.transform.position - transform.position;
            if (Data.Push)
            {
                var hitData = new HitData(Time.time, obj, contact, Data.Damage, direction.normalized, Data.PushDirection, Data.PushPower);
                dmg.PushAway(hitData);
            }
            else
            {
                var hitData = new HitData(Time.time, obj, contact, Data.Damage, direction.normalized);
                dmg.DealDamage(hitData);
            }
        }
    }
}
