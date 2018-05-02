using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
    public abstract class Game<TGame> : Singleton<TGame> where TGame : Game<TGame>
    {
        public bool InitOnStart;

        public SceneLoader Loader;

        public DebugConsole Console;

        public ControllSystem Controllers;

        public GameState StartState;

        public GameState CurrentState { get; protected set; }
        public GameState PreviousState { get; protected set; }

        protected GameState NextState;

        public List<GameState> AllStates { get; protected set; }

        void Start()
        {
            if (!InitOnStart)
                return;

            Init();
        }

        private void RegisterConsoleCommands()
        {
            Console.RegisterCommand("exit", "closes console", (_) => 
            {
                Console.Close(); return true;
            });

            Console.RegisterCommand("quit", "quits game", (_) =>
            {
                QuitGame(); return true;
            });

            Console.RegisterCommand("restart", "restarts game", (_) =>
            {
                RestartGame(); return true;
            });
        }

        public void Init()
        {
            RegisterConsoleCommands();
            
            if (AllStates != null)
                AllStates.Clear();
            else
                AllStates = new List<GameState>();

            AllStates.AddRange(GetComponentsInChildren<GameState>());
            
            if (!Controllers.InitOnStart)
                Controllers.Init();

            if (Loader)
            {
                Loader.OnSceneLoad -= SceneLoaded;
                Loader.OnSceneLoad += SceneLoaded;

                Loader.StartLoadScene(Loader.BaseScene);                
            }
            else
            {
                SceneLoaded();
            }
        }

        public abstract bool IsPlaying();

        protected virtual void OnSceneLoad()
        { }

        private void SceneLoaded()
        {
            SwitchState(StartState);
            OnSceneLoad();
            
            gameObject.BroadcastToAll("OnLevelLoaded");
        }

        public void SwitchState(GameState state)
        {
            if (NextState)
                Debug.LogWarningFormat(
                    "Changed state twice in this frame! From '{0}' to '{1}' and now to '{2}'", 
                    CurrentState, NextState, state
                );
            
            NextState = state;
        }

        public void SwitchState<TState>() where TState : GameState
        {
            if (NextState)
                Debug.LogWarningFormat(
                    "Changed state twice in this frame! From '{0}' to '{1}' and now to '{2}'", 
                    CurrentState, NextState, nameof(TState)
                );

            NextState = FindState<TState>();
        }

        public void SwitchStateImmediate(GameState state)
        {
            if (state != CurrentState)
                PreviousState = CurrentState;

            if (CurrentState != null) CurrentState.DoEnd();
            CurrentState = state;
            if (CurrentState != null) CurrentState.DoStart();
        }

        public void SwitchStateImmediate<TState>() where TState : GameState
        {
            SwitchStateImmediate(FindState<TState>());
        }

        public TState FindState<TState>() where TState : GameState
        {
            return (TState) AllStates.FirstOrDefault(s => s is TState);
        }

        public void QuitGame()
        {
            gameObject.BroadcastToAll("OnLevelCleanUp");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }

        public void RestartGame()
        {
            StopAllCoroutines();

            gameObject.BroadcastToAll("OnLevelCleanUp");

            if (Loader)
                Loader.StartLoadScene(Loader.CurrentScene);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void DestroyPersistent()
        {
            foreach (var pers in FindObjectsOfType<Persistent>())
            {
                pers.DestroyOnExit();
            }
        }

        void Update()
        {
            if (Loader && !Loader.IsReady)
                return;

            if (NextState)
            {
                var desiredState = NextState;
                
                NextState = null;
                
                SwitchStateImmediate(desiredState);
            }

            if (CurrentState != null)
            {
                CurrentState.Tick();
            }

            Controllers.Tick();
        }

        void FixedUpdate()
        {
            if (Loader && !Loader.IsReady)
                return;

            if (CurrentState != null)
            {
                CurrentState.FixedTick();
            }

            Controllers.FixedTick();
        }

        void LateUpdate()
        {
            if (Loader && !Loader.IsReady)
                return;

            if (CurrentState != null)
            {
                CurrentState.LateTick();
            }
            
            Controllers.LateTick();
        }
    }
}
