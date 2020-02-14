using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ILevelDependable
{
    void OnLevelCleanUp();
    void OnLevelLoaded();
    void OnPreLevelLoaded();
}

public interface ILoadingDependable
{
    void PreOnLoadingScreenOn();
    void PostOnLoadingScreenOn();

    void PreOnLoadingScreenOff();
    void PostOnLoadingScreenOff();
}
