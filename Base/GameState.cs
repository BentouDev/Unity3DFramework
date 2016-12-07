using UnityEngine;

namespace Framework
{
    public abstract class GameState<T> : MonoBehaviour, ILevelDependable where T : Game<T>
    {
        protected T Game;

        void Start()
        {
            Game = FindObjectOfType<T>();
        }

        public abstract bool IsPlayable();

        public abstract void OnLevelLoaded();

        public abstract void OnStart();

        public abstract void OnRun();

        public abstract void OnEnd();

        public virtual void OnLevelCleanUp()
        {

        }
    }
}
