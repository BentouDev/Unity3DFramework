using UnityEngine;

public abstract class GameState : MonoBehaviour, ILevelDependable
{
    protected MainGame Game;

    void Start()
    {
        Game = FindObjectOfType<MainGame>();
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
