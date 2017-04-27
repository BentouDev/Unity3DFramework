using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class BasePawn : MonoBehaviour, ITickable
    {
        public void Tick()
        {
            OnTick();
        }

        public void FixedTick()
        {
            OnFixedTick();
        }

        public void LateTick()
        {
            OnLateTick();
        }

        protected virtual void OnTick()
        { }

        protected virtual void OnFixedTick()
        { }

        protected virtual void OnLateTick()
        { }
    }
}
