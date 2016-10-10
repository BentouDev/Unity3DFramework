using UnityEngine;
using System.Collections;

public interface IDamageable
{
    void DealDamage(HitData data);
    void PushAway(HitData data);
    int GetCurrentHealth();
    int GetMaxHealth();
}
