using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
    public static class BaseGame
    {
        public static IGame Instance { get; internal set; }
    }

    public interface IGame
    {
        void SwitchState(GameState state);
        void SwitchState<TState>() where TState : GameState;
        void SwitchStateImmediate(GameState state);
        void SwitchStateImmediate<TState>() where TState : GameState;

        TState FindState<TState>() where TState : GameState;

        ControllSystem GetControllers();
        GUIController  GetGUI();
        SceneLoader    GetLoader();

        bool IsPlaying();
        void QuitGame();
        void RestartGame();
    }
    
    public abstract class Game<TGame> : Singleton<TGame>, IGame, ISingletonInstanceListener where TGame : Game<TGame>
    {
        public bool InitOnStart;
        
        [Validate("ValidateLoadOnStart")]
        public bool LoadOnStart;

        [RequireValue]
        public DataSetInstance GlobalVars;

        public SceneLoader Loader;

        public ControllSystem Controllers;

        [RequireValue]
        public DebugConsole Console;

        [RequireValue]
        public GUIController GUI;

        [RequireValue]
        public GameState StartState;

        public GameState CurrentState { get; protected set; }
        public GameState PreviousState { get; protected set; }

        protected GameState NextState;

        public List<GameState> AllStates { get; protected set; }

        public ControllSystem GetControllers() => Controllers;

        public GUIController GetGUI() => GUI;

        public SceneLoader GetLoader() => Loader;

        public void OnSetInstance()
        {
            BaseGame.Instance = this;
        }

        void Start()
        {
            if (!InitOnStart)
                return;

            Init();
        }

        private void RegisterConsoleCommands()
        {
            Console.RegisterVariableDataSet(GlobalVars);
            
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

        void ObtainStates()
        {
            if (AllStates != null)
                AllStates.Clear();
            else
                AllStates = new List<GameState>();

            AllStates.AddRange(FindObjectsOfType<GameState>());            
        }

        public void Init()
        {
            Instance.OnSetInstance();
            
            RegisterConsoleCommands();

            if (Loader)
            {
                Loader.OnSceneLoad -= SceneLoaded;
                Loader.OnSceneLoad += SceneLoaded;

                if (LoadOnStart) 
                    Loader.StartLoadScene(Loader.SceneToLoad);
                else
                    SceneLoaded();
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
            ObtainStates();
            
            //var go = GameObject.FindWithTag("MainState");
            //var levelState = go ? go.GetComponent<GameState>() : null;
            var levelState = AllStates.FirstOrDefault(s => s.gameObject.scene.path != Loader.BaseScene
                                                          && s.CompareTag("MainState"));

            gameObject.BroadcastToAll("OnPreLevelLoaded");
            
            if (NextState)
            {
                SwitchState(NextState);
            }
            else if (levelState)
            {                
                SwitchState(levelState);
            }
            else
            {
                SwitchState(StartState);
            }

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
            GlobalQuitGame();
        }

        public static void GlobalQuitGame()
        {
            if (Game<TGame>.Instance)
                Game<TGame>.Instance.gameObject.BroadcastToAll("OnLevelCleanUp");

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

                return;
            }

            if (CurrentState != null)
                CurrentState.Tick();

            if (Controllers)
                Controllers.Tick();
        }

        void FixedUpdate()
        {
            if (Loader && !Loader.IsReady)
                return;

            if (CurrentState != null)
                CurrentState.FixedTick();
            
            if (Controllers)
                Controllers.FixedTick();
        }

        void LateUpdate()
        {
            if (Loader && !Loader.IsReady)
                return;

            if (CurrentState != null)
                CurrentState.LateTick();
            
            if (Controllers)
                Controllers.LateTick();
        }

        public ValidationResult ValidateLoadOnStart()
        {
            if (LoadOnStart && !Loader)
                return new ValidationResult(ValidationStatus.Warning, "No scene loader!");

            if (LoadOnStart && Loader && !Loader.SceneToLoad.HasValue())
                return new ValidationResult(ValidationStatus.Warning, "No scene to load, check SceneLoader");

            return ValidationResult.Ok;
        }
    }
}
