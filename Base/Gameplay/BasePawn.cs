using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class BasePawn<T> : Controllable<T>
    {
        public virtual void OnLevelLoaded()
        {

        }

        public virtual void OnLevelCleanUp()
        {

        }

        public override bool ProcessInput(T processedInput)
        {
            return false;
        }

        public virtual void OnUpdate()
        {

        }
    }
}
