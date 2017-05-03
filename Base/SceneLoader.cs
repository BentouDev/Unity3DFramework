using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Framework
{
    public class SceneLoader : MonoBehaviour
    {
        public int BaseScene;

        private static bool _waitForLoad;
        private static int _nextScene;
        
        public int CurrentScene { get; private set; }
        public bool IsReady { get; private set; }

        public delegate void SceneLoad();

        public event SceneLoad OnSceneLoad;
        
        private void EndLoadScene()
        {
            CurrentScene = _nextScene;
            IsReady = true;
            _waitForLoad = false;
            Debug.Log("Loaded scene " + _nextScene + "!");

            if (OnSceneLoad != null)
                OnSceneLoad();
        }

        private IEnumerator ExecuteLoading(int scene)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
            yield return operation;

            //while (!_waitForLoad || IsReady)
            //{
            //    yield return null;
            //}

            EndLoadScene();
        }

        public void StartLoadScene(int i)
        {
            if (_waitForLoad || i == 0)
                return;

            _nextScene = i;
            IsReady = false;
            _waitForLoad = true;
            Debug.Log("Loading scene " + i + " ...");

            StartCoroutine(ExecuteLoading(i));
        }
    }
}