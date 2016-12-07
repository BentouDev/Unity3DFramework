using UnityEngine;

namespace Framework
{
    public interface IGameplayControlled
    {
        void OnPreBeginPlay();
        void OnPostBeginPlay();

        void OnPausePlay();
        void OnResumePlay();

        void OnEndPlay();

        void OnGameLose();
        void OnGameWin();
        void OnGameWithdraw();

        void OnLevelLoaded();
        void OnLevelCleanUp();
    }
    
    public abstract class Game<T> : Singleton<T> where T : Game<T>
    {
        public SceneLoader Manager;

        public abstract bool IsInGame();

        public abstract bool IsPlaying();

        public abstract bool IsJustStarted();

        internal void OnPreBeginPlay()
        {
            var allObjs = FindObjectsOfType<GameObject>();
            foreach (GameObject allObj in allObjs)
            {
                var allGameControlled = allObj.GetInterfaces<IGameplayControlled>();
                if (allGameControlled != null && allGameControlled.Length > 0)
                {
                    foreach (IGameplayControlled gameplayControlled in allGameControlled)
                    {
                        gameplayControlled.OnPreBeginPlay();
                    }
                }
            }
        }

        internal void OnPostBeginPlay()
        {
            var allObjs = FindObjectsOfType<GameObject>();
            foreach (GameObject allObj in allObjs)
            {
                var allGameControlled = allObj.GetInterfaces<IGameplayControlled>();
                if (allGameControlled != null && allGameControlled.Length > 0)
                {
                    foreach (IGameplayControlled gameplayControlled in allGameControlled)
                    {
                        gameplayControlled.OnPostBeginPlay();
                    }
                }
            }
        }

        internal void OnPausePlay()
        {
            var allObjs = FindObjectsOfType<GameObject>();
            foreach (GameObject allObj in allObjs)
            {
                var allGameControlled = allObj.GetInterfaces<IGameplayControlled>();
                if (allGameControlled != null && allGameControlled.Length > 0)
                {
                    foreach (IGameplayControlled gameplayControlled in allGameControlled)
                    {
                        gameplayControlled.OnPausePlay();
                    }
                }
            }
        }

        internal void OnResumePlay()
        {
            var allObjs = FindObjectsOfType<GameObject>();
            foreach (GameObject allObj in allObjs)
            {
                var allGameControlled = allObj.GetInterfaces<IGameplayControlled>();
                if (allGameControlled != null && allGameControlled.Length > 0)
                {
                    foreach (IGameplayControlled gameplayControlled in allGameControlled)
                    {
                        gameplayControlled.OnResumePlay();
                    }
                }
            }
        }

        internal void OnEndPlay()
        {
            var allObjs = FindObjectsOfType<GameObject>();
            foreach (GameObject allObj in allObjs)
            {
                var allGameControlled = allObj.GetInterfaces<IGameplayControlled>();
                if (allGameControlled != null && allGameControlled.Length > 0)
                {
                    foreach (IGameplayControlled gameplayControlled in allGameControlled)
                    {
                        gameplayControlled.OnEndPlay();
                    }
                }
            }
        }

        internal void OnGameLose()
        {
            var allObjs = FindObjectsOfType<GameObject>();
            foreach (GameObject allObj in allObjs)
            {
                var allGameControlled = allObj.GetInterfaces<IGameplayControlled>();
                if (allGameControlled != null && allGameControlled.Length > 0)
                {
                    foreach (IGameplayControlled gameplayControlled in allGameControlled)
                    {
                        gameplayControlled.OnGameLose();
                    }
                }
            }
        }

        internal void OnGameWin()
        {
            var allObjs = FindObjectsOfType<GameObject>();
            foreach (GameObject allObj in allObjs)
            {
                var allGameControlled = allObj.GetInterfaces<IGameplayControlled>();
                if (allGameControlled != null && allGameControlled.Length > 0)
                {
                    foreach (IGameplayControlled gameplayControlled in allGameControlled)
                    {
                        gameplayControlled.OnGameWin();
                    }
                }
            }
        }

        internal void OnGameWithdraw()
        {
            var allObjs = FindObjectsOfType<GameObject>();
            foreach (GameObject allObj in allObjs)
            {
                var allGameControlled = allObj.GetInterfaces<IGameplayControlled>();
                if (allGameControlled != null && allGameControlled.Length > 0)
                {
                    foreach (IGameplayControlled gameplayControlled in allGameControlled)
                    {
                        gameplayControlled.OnGameWithdraw();
                    }
                }
            }
        }

        internal void OnLevelLoaded()
        {
            var allObjs = FindObjectsOfType<GameObject>();
            foreach (GameObject allObj in allObjs)
            {
                var allGameControlled = allObj.GetInterfaces<IGameplayControlled>();
                if (allGameControlled != null && allGameControlled.Length > 0)
                {
                    foreach (IGameplayControlled gameplayControlled in allGameControlled)
                    {
                        gameplayControlled.OnLevelLoaded();
                    }
                }
            }
        }

        internal void OnLevelCleanUp()
        {
            var allObjs = FindObjectsOfType<GameObject>();
            foreach (GameObject allObj in allObjs)
            {
                var allGameControlled = allObj.GetInterfaces<IGameplayControlled>();
                if (allGameControlled != null && allGameControlled.Length > 0)
                {
                    foreach (IGameplayControlled gameplayControlled in allGameControlled)
                    {
                        gameplayControlled.OnLevelCleanUp();
                    }
                }
            }
        }
    }
}
