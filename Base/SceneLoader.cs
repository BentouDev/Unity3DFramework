using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Framework
{
    public class SceneLoader : MonoBehaviour
    {
        public int BaseScene;

        private static bool WaitForLoad;
        private static int NextScene;

        public int SceneToLoad;
        public int CurrentScene { get; private set; }
        public bool IsReady { get; private set; }

        public delegate void SceneLoad();

        public event SceneLoad OnSceneLoad;

        void Start()
        {
            StartLoadScene(BaseScene);
        }

        void Update()
        {
            //    if (!Application.isLoadingLevel && WaitForLoad && !Game.IsReady)
            //    {
            //        EndLoadScene();
            //    }
        }

        private void EndLoadScene()
        {
            CurrentScene = NextScene;
            IsReady = true;
            WaitForLoad = false;
            Debug.Log("Loaded scene " + NextScene + "!");

            if (OnSceneLoad != null)
                OnSceneLoad();
        }

        private IEnumerator ExecuteLoading(int scene)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
            yield return operation;

            while (!WaitForLoad || IsReady)
            {
                yield return null;
            }

            EndLoadScene();
        }

        public void StartLoadScene(int i)
        {
            if (WaitForLoad || i == 0)
                return;

            NextScene = i;
            IsReady = false;
            WaitForLoad = true;
            Debug.Log("Loading scene " + i + " ...");

            StartCoroutine(ExecuteLoading(i));
        }
    }
}