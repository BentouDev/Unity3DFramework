using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SocketManager : MonoBehaviour
{
    [SerializeField] 
    public List<CustomProperties> Sockets = new List<CustomProperties>();
    
    void OnValidate()
    {
        var allSockets = GetComponentsInChildren<CustomProperties>();
        foreach (CustomProperties socket in allSockets)
        {
            if (!Sockets.Contains(socket))
                Sockets.Add(socket);
        }
    }
}
