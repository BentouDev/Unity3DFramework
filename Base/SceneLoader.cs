using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public MainGame Game;

    public int BaseScene;

    private static bool WaitForLoad;
    private static int NextScene;

    public int SceneToLoad;

    public delegate void SceneLoad();
    public event SceneLoad OnSceneLoad;

    void Start()
    {
        if (Game == null)
            Game = FindObjectOfType<MainGame>();

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
        Game.CurrentScene = NextScene;
        Game.IsReady = true;
        WaitForLoad = false;
        Debug.Log("Loaded scene " + NextScene + "!");

        if (OnSceneLoad != null)
            OnSceneLoad();
    }

    private IEnumerator ExecuteLoading(int scene)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
        yield return operation;

        while (!WaitForLoad || Game.IsReady)
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
        WaitForLoad = true;
        Debug.Log("Loading scene " + i + " ...");

        StartCoroutine(ExecuteLoading(i));
    }
}
