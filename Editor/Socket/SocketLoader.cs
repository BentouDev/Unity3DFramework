using UnityEditor;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public class SocketLoader : AssetPostprocessor
{
    void OnPostprocessGameObjectWithUserProperties(
        GameObject go, 
        string[] names, 
        System.Object[] values
    )
    {
        var parent = FindParent(go);
        if (parent)
        {
            var socket = go.GetOrAddComponent<CustomProperties>();
            for (int i = 0; i < names.Length; i++)
            {
                socket.AddProperty(names[i], values[i]);
            }
        }
        else
        {
            Debug.LogErrorFormat("Unable to get prefab parent for {0}!", go);
            
            for (int i = 0; i < names.Length; i++)
            {
                string propertyName = names[i];
                object propertyValue = values[i];
                Debug.Log("Propname: " + propertyName + " value: " +propertyValue);
            }
        }
    }

    static GameObject FindParent(GameObject go)
    {
        GameObject parent = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
        if (parent == null)
            parent = PrefabUtility.GetPrefabInstanceHandle(go) as GameObject;

        if (parent == null)
        {
            parent = go;

            while (parent.transform.parent != null)
                parent = parent.transform.parent.gameObject;
        }

        return parent;
    }
}
