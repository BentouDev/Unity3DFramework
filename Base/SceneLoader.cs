using System;
using RSG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine.Events;
using UnityEngine.Experimental.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Framework
{
    public class SceneLoader : Framework.BaseBehaviour
    {
        [RequireValue]
        public SceneReference BaseScene;
        
        [RequireValue]
        public SceneReference LoadingScreen;
        
        public SceneReference SceneToLoad;

        private static bool _isLoading;
        private static SceneReference _nextScene;
        
        public SceneReference CurrentScene { get; private set; }
        public bool IsReady => !_isLoading;

        public delegate void SceneLoad();

        public event SceneLoad OnSceneLoad;
       
        private void EndLoadScene(bool already_loaded = false)
        {
            CurrentScene = _nextScene;

            if (!already_loaded)
                Debug.Log("Loaded scene " + SceneManager.GetActiveScene().buildIndex + "!");

            if (OnSceneLoad != null)
                OnSceneLoad();

            _isLoading = false;
        }

        public void PrintScenes()
        {
            string output = "Scenes:\n";
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                output += SceneManager.GetSceneAt(i).path + "\n";
            }
            
            Debug.Log(output);
        }

        private IEnumerator ExecuteLoading(string scene, UnityAction onFinish = null)
        {
            Debug.LogFormat("Async loading scene '{0}'...", scene);

            AsyncOperation operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            yield return operation;

            Debug.LogFormat("Loaded scene '{0}'!", scene);

            onFinish?.Invoke();
        }

        private IEnumerator ExecuteUnload(string scene, UnityAction onFinish = null)
        {
            Debug.LogFormat("Async unloading scene '{0}'...", scene);

            var operation = SceneManager.UnloadSceneAsync(scene);
            yield return operation;
            
            Debug.LogFormat("Unloaded scene '{0}'!", scene);
            
            onFinish?.Invoke();
        }
        
        IEnumerator CallAfterSec(float seconds, UnityAction func)
        {
            yield return new WaitForSeconds(seconds);
            func?.Invoke();
        }

//        public delegate void TheFunc();
//        public delegate IPromise PromiseFunc();
//
//        public interface IPromise
//        {
//            void Resolve();
//            IPromise Then(TheFunc func);
//            IPromise Then(PromiseFunc func);
//        }
//
//        public class Promise : IPromise
//        {
//            TheFunc OnResolve;
//            TheFunc MyOperation;
//
//            public Promise()
//            { }
//
//            public Promise(TheFunc onResolve)
//            {
//                MyOperation = onResolve;
//                OnResolve = () => MyOperation();
//            }
//            
//            public void Resolve()
//            {
//                OnResolve?.Invoke();
//            }
//
//            public IPromise Then(TheFunc func)
//            {
//                var promise = new Promise(func);
//                OnResolve = () =>
//                {
//                    MyOperation?.Invoke();
//                    promise.Resolve();
//                };
//                
//                return promise;
//            }
//
//            public IPromise Then(PromiseFunc func)
//            {
//                var promise = new Promise();
//                OnResolve = () =>
//                {
//                    MyOperation?.Invoke();
//                    var resultPromise = func();
//                    resultPromise.Then(() => { promise.Resolve(); });
//                };
//
//                return promise;
//            }
//
//            public static IPromise All(IEnumerable<IPromise> promises)
//            {
//                IPromise promise = new Promise();                
//                int count = promises.Count();
//                foreach (IPromise element in promises)
//                {
//                    element.Then
//                    (
//                        () =>
//                        {
//                            count--;
//                            if (count == 0)
//                            {
//                                promise.Resolve();
//                            }
//                        }
//                    );
//                }   
//
//                return promise;
//            }
//        }

        private IPromise FadeToBlack()
        {
            Debug.LogFormat("Fade...");
            var promise = new Promise();

            if (!BaseGame.Instance.GetGUI()
            ||  !BaseGame.Instance.GetGUI().PlayFade(() => promise.Resolve()))
            {
                promise.Resolve();
            }

            return promise;
        }

        private IPromise UnfadeFromBlack()
        {
            Debug.LogFormat("Unfade...");
            var promise = new Promise();

            if (!BaseGame.Instance.GetGUI() 
            ||  !BaseGame.Instance.GetGUI().PlayUnfade(() => promise.Resolve()))
            {
                promise.Resolve();
            }

            return promise;
        }

        private IPromise LoadScene(string index)
        {
            var promise = new Promise();
            StartCoroutine(ExecuteLoading(index, () => promise.Resolve()));
            return promise;
        }

        private IPromise UnloadScene(string index)
        {
            var promise = new Promise();
            StartCoroutine(ExecuteUnload(index, () => promise.Resolve()));
            return promise;            
        }

        private IPromise UnloadStrayScenes()
        {
            var promise = new Promise();
            var sceneUnloaders = GetLoadedScenes().Select(s => UnloadScene(s));
            if (sceneUnloaders.Any())
            {
                Promise.All(
                    sceneUnloaders
                ).Then(() => promise.Resolve());                
            }
            else
            {
                StartCoroutine(CallAfterSec(1, () => { promise.Resolve(); }));
            }


            return promise;
        }

        private void CommenceLoading(SceneReference targetScene)
        {
            FadeToBlack().Then(() =>
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByPath(BaseScene.SceneName));
            }).Then(() => {
                gameObject.BroadcastToAll("PreOnLoadingScreenOn");
            }).Then(
                () => LoadScene(LoadingScreen)
            ).Then(() => {
                gameObject.BroadcastToAll("PostOnLoadingScreenOn");
            }).Then(
                UnfadeFromBlack
            ).Then(
                UnloadStrayScenes
            ).Then(
                () => LoadScene(targetScene)
            ).Then(() => {
                gameObject.BroadcastToAll("PreOnLoadingScreenOff");
            }).Then(
                FadeToBlack
            ).Then(
                () => UnloadScene(LoadingScreen)
            ).Then(() => {
                gameObject.BroadcastToAll("PostOnLoadingScreenOff");
                SceneManager.SetActiveScene(SceneManager.GetSceneByPath(targetScene.SceneName));
            }).Then(
                UnfadeFromBlack
            ).Then
            (
                () => EndLoadScene()
            ).Catch(exception =>
            {
                Debug.LogError($"Scene loading failed with following exception: {exception.ToString()}");
            });
        }

        private IList<string> GetLoadedScenes()
        {
            List<string> loadedScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                
                if (scene.path == LoadingScreen.SceneName)
                    continue;
                if (scene.path == BaseScene.SceneName)
                    continue;

                loadedScenes.Add(scene.path);
            }

            return loadedScenes;
        }
        
        public IList<Scene> GetLoadedGameScenes()
        {
            List<Scene> loadedScenes = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                
                if (scene.path == LoadingScreen.SceneName)
                    continue;
                if (scene.path == BaseScene.SceneName)
                    continue;

                loadedScenes.Add(scene);
            }

            return loadedScenes;
        }

        public void StartLoadScene(SceneReference desiredScene, bool force = false)
        {
            if (_isLoading)
                return;

            BaseGame.Instance.SwitchStateImmediate<GameLoadingScreenState>();

            _nextScene = desiredScene;
            
            if (_isLoading || (!force && SceneManager.GetActiveScene().path == desiredScene))
            {
                Debug.LogFormat("Scene '{0}' already loaded!", desiredScene);
                EndLoadScene(already_loaded: true);
                return;
            }

            _isLoading = true;
            Debug.LogFormat("Loading scene '{0}' ...", desiredScene);

            // StartCoroutine(ExecuteLoading(i, () => EndLoadScene()));
            
            CommenceLoading(desiredScene);
        }

        public IList<Scene> GetCurrentSystemScenes()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path == BaseScene.SceneName)
                return new []{SceneManager.GetSceneByPath(BaseScene), SceneManager.GetSceneByPath(LoadingScreen)};
            return new []{SceneManager.GetSceneByPath(BaseScene)};
        }
    }
}