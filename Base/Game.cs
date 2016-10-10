//using SoM.NetworkManager;
using UnityEngine;

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

public abstract class Game<T> : Singleton<T> where T : MonoBehaviour
{
    public abstract bool IsInGame();

    public abstract bool IsPlaying();

    public abstract bool IsJustStarted();

    internal void OnPreBeginPlay()
    {
        //NetworkManager.SendDeviceInfo();
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            var inter = obj.GetComponents(typeof(IGameplayControlled)) as IGameplayControlled[];
            if (inter != null && inter.Length > 0)
                foreach (var ctrl in inter)
                    ctrl.OnPreBeginPlay();
        }
    }

    internal void OnPostBeginPlay()
    {
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            var inter = obj.GetComponents(typeof(IGameplayControlled)) as IGameplayControlled[];
            if (inter != null && inter.Length > 0)
                foreach (var ctrl in inter)
                    ctrl.OnPostBeginPlay();
        }
    }

    internal void OnPausePlay()
    {
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            var inter = obj.GetComponents(typeof(IGameplayControlled)) as IGameplayControlled[];
            if (inter != null && inter.Length > 0)
                foreach (var ctrl in inter)
                    ctrl.OnPausePlay();
        }
    }

    internal void OnResumePlay()
    {
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            var inter = obj.GetComponents(typeof(IGameplayControlled)) as IGameplayControlled[];
            foreach (var ctrl in inter)
                ctrl.OnResumePlay();
        }
    }

    internal void OnEndPlay()
    {
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            var inter = obj.GetComponents(typeof(IGameplayControlled)) as IGameplayControlled[];
            if (inter != null && inter.Length > 0)
                foreach (var ctrl in inter)
                    ctrl.OnEndPlay();
        }
    }

    internal void OnGameLose()
    {
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            var inter = obj.GetComponents(typeof(IGameplayControlled)) as IGameplayControlled[];
            if (inter != null && inter.Length > 0)
                foreach (var ctrl in inter)
                    ctrl.OnGameLose();
        }
    }

    internal void OnGameWin()
    {
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            var inter = obj.GetComponents(typeof(IGameplayControlled)) as IGameplayControlled[];
            if (inter != null && inter.Length > 0)
                foreach (var ctrl in inter)
                    ctrl.OnGameWin();
        }
    }

    internal void OnGameWithdraw()
    {
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            var inter = obj.GetComponents(typeof(IGameplayControlled)) as IGameplayControlled[];
            if (inter != null && inter.Length > 0)
                foreach (var ctrl in inter)
                    ctrl.OnGameWithdraw();
        }
    }

    internal void OnLevelLoaded()
    {
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            var inter = obj.GetComponents(typeof(IGameplayControlled)) as IGameplayControlled[];
            if (inter != null && inter.Length > 0)
                foreach (var ctrl in inter)
                {
                    ctrl.OnLevelLoaded();
                }

        }
    }

    internal void OnLevelCleanUp()
    {
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            var inter = obj.GetComponents(typeof(IGameplayControlled)) as IGameplayControlled[];
            if (inter != null && inter.Length > 0)
                foreach (var ctrl in inter)
                    ctrl.OnLevelCleanUp();
        }
    }
}
