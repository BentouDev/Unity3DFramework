using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    [System.Serializable]
    public abstract class DialogState : ScriptableObject
    {
        public void Begin()
        {
            OnBegin();
        }

        public void End()
        {
            OnEnd();
        }

        protected virtual void OnBegin()
        { }

        protected virtual void OnEnd()
        { }

        public virtual void DestroyChildren()
        { }
    }
}
