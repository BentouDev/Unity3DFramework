using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITickable
{
    void Tick();
    void FixedTick();
    void LateTick();
}
