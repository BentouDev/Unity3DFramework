using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Framework
{
    public class SceneLoader : MonoBehaviour
    {
        [FormerlySerializedAs("BaseScene")]
        public int SceneToLoad;

        private static bool _waitForLoad;
        private static int _nextScene;
        
        public int CurrentScene { get; private set; }
        public bool IsReady { get; private set; }

        public delegate void SceneLoad();

        public event SceneLoad OnSceneLoad;
       
        private void EndLoadScene(bool already_loaded = false)
        {
            CurrentScene = _nextScene;
            IsReady      = true;
            _waitForLoad = false;
            
            if (!already_loaded)
                Debug.Log("Loaded scene " + SceneManager.GetActiveScene().buildIndex + "!");

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
            _nextScene = i;
            
            if (_waitForLoad || SceneManager.GetActiveScene().buildIndex == i)
            {
                Debug.Log("Scene " + i + " already loaded!");
                EndLoadScene(already_loaded: true);
                return;
            }

            IsReady = false;
            _waitForLoad = true;
            Debug.Log("Loading scene " + i + " ...");

            StartCoroutine(ExecuteLoading(i));
        }
    }
}