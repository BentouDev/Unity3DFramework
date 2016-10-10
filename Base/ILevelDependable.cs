using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ILevelDependable
{
    void OnLevelCleanUp();
    void OnLevelLoaded();
}
