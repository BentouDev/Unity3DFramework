using UnityEngine;

namespace Framework
{
    public abstract class GameState : MonoBehaviour, ITickable
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

        public void DoStart()
        {
            OnStart();
        }

        public void DoEnd()
        {
            OnEnd();
        }

        protected virtual void OnEnd()
        { }

        protected virtual void OnStart()
        { }
    }
}
