using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    private static bool _showKillzones;
    private const string _showKillzonesMenuName = "Gameplay/ShowKillzones";

    static KillZone()
    {
        _showKillzones = EditorPrefs.GetBool(_showKillzonesMenuName, false);

        EditorApplication.delayCall += () => {
            HandleKillzoneDisplay(_showKillzones);
        };
    }

    private static void HandleKillzoneDisplay(bool value)
    {
        Menu.SetChecked(_showKillzonesMenuName, value);
        EditorPrefs.SetBool(_showKillzonesMenuName, value);

        _showKillzones = value;
    }

    [MenuItem(_showKillzonesMenuName)]
    public static void ToggleShowKillzones()
    {
        HandleKillzoneDisplay(!_showKillzones);
    }

    private void OnDrawGizmos()
    {
        if (!_showKillzones)
            return;
        
    }

    private void OnDrawGizmosSelected()
    {
        
    }

    private void DrawKillzone()
    {

    }
}
